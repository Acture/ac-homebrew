using System.Reflection;
using System.Text.Json;

namespace AssemblyInspector;

public static class Cli
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        try
        {
            if (args is ["--help"])
            {
                output.WriteLine("Usage: --assembly FILE --type EXACT_NAME [--type EXACT_NAME ...] [--member NAME] [--reference DIR ...] [--json]");
                return 0;
            }

            string? assembly = null;
            string? member = null;
            var types = new List<string>();
            var references = new List<string>();
            var json = false;
            for (var i = 0; i < args.Length; i++)
            {
                var option = args[i];
                if (option == "--json")
                {
                    if (json) throw new ArgumentException("--json was supplied more than once.");
                    json = true;
                    continue;
                }
                if (option is not ("--assembly" or "--type" or "--member" or "--reference"))
                    throw new ArgumentException($"Unknown option: {option}. Use --help for usage.");
                if (++i == args.Length || string.IsNullOrWhiteSpace(args[i]) || args[i].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException($"Missing value for {option}.");
                var value = args[i];
                switch (option)
                {
                    case "--assembly":
                        if (assembly is not null) throw new ArgumentException("--assembly was supplied more than once.");
                        assembly = value;
                        break;
                    case "--type": types.Add(value); break;
                    case "--member":
                        if (member is not null) throw new ArgumentException("--member was supplied more than once.");
                        member = value;
                        break;
                    case "--reference": references.Add(value); break;
                }
            }

            if (assembly is null) throw new ArgumentException("--assembly is required.");
            if (types.Count == 0) throw new ArgumentException("At least one --type is required.");
            var report = Inspector.Inspect(assembly, types, member, references);
            var text = json
                ? JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                })
                : Inspector.ToText(report);
            output.WriteLine(text);
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException
            or IOException or UnauthorizedAccessException or BadImageFormatException or TypeLoadException
            or NotSupportedException or CustomAttributeFormatException or FormatException)
        {
            error.WriteLine($"Assembly inspection failed: {exception.Message}");
            return 1;
        }
    }
}
