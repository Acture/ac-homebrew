using System.Reflection;
using System.Security.Cryptography;

namespace AssemblyInspector;

internal sealed class MetadataResolver : MetadataAssemblyResolver
{
    private sealed record Candidate(string Path, AssemblyName Identity);
    private readonly Dictionary<string, List<Candidate>> candidates = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Candidate> framework = new(StringComparer.OrdinalIgnoreCase);

    public string CoreAssemblyName { get; }

    public MetadataResolver(string assemblyPath, List<string> references)
    {
        var targetIdentity = AssemblyName.GetAssemblyName(assemblyPath);
        AddCandidate(new Candidate(assemblyPath, targetIdentity));
        var directories = new[] { Path.GetDirectoryName(assemblyPath)! }.Concat(references.Select(Path.GetFullPath))
            .Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException($"Reference directory does not exist: {directory}");
            foreach (var path in Directory.EnumerateFiles(directory, "*.dll").Order(StringComparer.Ordinal))
            {
                var candidate = ReadCandidate(path);
                if (candidate is not null) AddCandidate(candidate);
            }
        }
        // Only host framework DLLs are fallback references, never the invoking application's dependencies.
        foreach (var path in Directory.EnumerateFiles(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "*.dll"))
        {
            var candidate = ReadCandidate(path);
            if (candidate is not null) framework.Add(candidate.Identity.Name!, candidate);
        }
        CoreAssemblyName = candidates.ContainsKey("mscorlib") ? "mscorlib" : "System.Private.CoreLib";
    }

    public override Assembly? Resolve(MetadataLoadContext context, AssemblyName assemblyName)
    {
        var name = assemblyName.Name!;
        if (candidates.TryGetValue(name, out var matches))
        {
            var first = matches[0];
            foreach (var other in matches.Skip(1))
            {
                if (first.Identity.FullName != other.Identity.FullName
                    || !SHA256.HashData(File.ReadAllBytes(first.Path)).SequenceEqual(SHA256.HashData(File.ReadAllBytes(other.Path))))
                    throw new InvalidOperationException($"Conflicting metadata dependency '{name}': {first.Path} and {other.Path}. Remove the ambiguity.");
            }
            return context.LoadFromAssemblyPath(first.Path);
        }
        if (framework.TryGetValue(name, out var fallback)) return context.LoadFromAssemblyPath(fallback.Path);
        throw new FileNotFoundException($"Metadata dependency '{assemblyName.FullName}' was not found. Supply its directory with --reference.");
    }

    private void AddCandidate(Candidate candidate)
    {
        var name = candidate.Identity.Name!;
        if (!candidates.TryGetValue(name, out var matches)) candidates.Add(name, matches = []);
        if (!matches.Any(item => string.Equals(item.Path, candidate.Path,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
            matches.Add(candidate);
    }

    private static Candidate? ReadCandidate(string path)
    {
        try { return new Candidate(path, AssemblyName.GetAssemblyName(path)); }
        // Native DLLs can share a game directory with managed assemblies.
        catch (BadImageFormatException) { return null; }
    }
}
