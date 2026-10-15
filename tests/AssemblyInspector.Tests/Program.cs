using System.Text.Json;
using AssemblyInspector;
using InspectorFixture;

var failures = 0;
var cases = 0;
var fixture = typeof(Dangerous<>).Assembly.Location;
var dependency = typeof(InspectorDependency.ExternalBase).Assembly.Location;
var typeName = "InspectorFixture.Dangerous`1";
var temp = Path.Combine(Path.GetTempPath(), "assembly-inspector-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temp);
var marker = Path.Combine(temp, "executed.txt");
var previousMarker = Environment.GetEnvironmentVariable("ASSEMBLY_INSPECTOR_TEST_MARKER");
Environment.SetEnvironmentVariable("ASSEMBLY_INSPECTOR_TEST_MARKER", marker);

void Expect(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

void Check(string name, Action test)
{
    cases++;
    try { test(); }
    catch (Exception error)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {name}: {error.Message}");
    }
}

(int Code, string Output, string Error) Run(params string[] args)
{
    using var output = new StringWriter();
    using var error = new StringWriter();
    var code = Cli.Run(args, output, error);
    return (code, output.ToString(), error.ToString());
}

JsonDocument Inspect(params string[] extra)
{
    var result = Run(["--assembly", fixture, "--type", typeName, "--json", .. extra]);
    Expect(result.Code == 0, result.Error);
    Expect(result.Error.Length == 0, "Successful inspection wrote stderr");
    Expect(!File.Exists(marker), "Inspection executed fixture code");
    return JsonDocument.Parse(result.Output);
}

void Fails(string expected, params string[] args)
{
    var result = Run(args);
    Expect(result.Code != 0, "Invalid request succeeded");
    Expect(result.Output.Length == 0, "Failed request emitted partial output");
    Expect(result.Error.Contains(expected, StringComparison.OrdinalIgnoreCase), result.Error);
}

try
{
    Check("hazard control detects execution", () =>
    {
        try { Hazard.Execute("control"); }
        catch (InvalidOperationException) { }
        Expect(File.ReadAllText(marker).Contains("control"), "Hazard marker did not work");
        File.Delete(marker);
    });

    Check("metadata inspection never executes target or attribute code", () =>
    {
        using var document = Inspect();
        var type = document.RootElement.GetProperty("types")[0];
        Expect(type.GetProperty("name").GetString() == typeName, "Wrong type");
        Expect(type.GetProperty("baseType").GetString() == "InspectorDependency.ExternalBase", "Missing base type");
        var attribute = type.GetProperty("attributes").EnumerateArray()
            .Single(item => item.GetProperty("type").GetString() == "InspectorFixture.DangerousAttribute");
        Expect(attribute.GetProperty("constructorArguments")[0].GetProperty("value").GetString() == "type", "Missing attribute argument");
        Expect(attribute.GetProperty("constructorArguments")[1].GetProperty("value").GetString() == "InspectorDependency.ExternalValue", "Missing attribute type argument");
        Expect(attribute.GetProperty("constructorArguments")[2].GetProperty("value").GetArrayLength() == 2, "Missing array argument");
        Expect(attribute.GetProperty("namedArguments")[0].GetProperty("name").GetString() == "Note", "Missing named argument");
        var members = type.GetProperty("members").EnumerateArray().ToArray();
        var field = members.Single(item => item.GetProperty("name").GetString() == "hidden");
        Expect(field.GetProperty("visibility").GetString() == "private" && field.GetProperty("isStatic").GetBoolean(), "Missing private static field");
        Expect(!field.TryGetProperty("value", out _), "Inspector attempted to report live field value");
        Expect(members.Any(item => item.GetProperty("kind").GetString() == "constructor"), "Missing constructor");
        Expect(members.Any(item => item.GetProperty("name").GetString() == ".cctor"), "Missing static initializer metadata");
        Expect(members.Any(item => item.GetProperty("kind").GetString() == "event"), "Missing event");
        Expect(!members.Any(item => item.GetProperty("name").GetString() == "ToString"), "Inherited member leaked");
        var property = members.Single(item => item.GetProperty("kind").GetString() == "property" && item.GetProperty("name").GetString() == "Explosive");
        Expect(property.GetProperty("accessors").GetArrayLength() == 2, "Missing property accessors");
        Expect(property.GetProperty("accessors")[1].GetProperty("visibility").GetString() == "private", "Lost setter visibility");
        var generic = members.Single(item => item.GetProperty("name").GetString() == "Convert");
        Expect(generic.GetProperty("genericParameters")[0].GetProperty("name").GetString() == "U", "Missing generic parameter");
        Expect(generic.GetProperty("parameters")[0].GetProperty("type").GetString() == "T&", "Missing byref parameter");
        Expect(generic.GetProperty("parameters")[1].GetProperty("isOut").GetBoolean(), "Missing out parameter");
        Expect(type.GetProperty("genericParameters")[0].GetProperty("attributes").GetString()!.Contains("ReferenceTypeConstraint"), "Missing constraints");
        Expect(document.RootElement.GetProperty("dependencies").EnumerateArray()
            .Any(item => item.GetProperty("name").GetString()!.StartsWith("AssemblyInspector.Dependency,")), "Missing resolved dependency");
    });

    Check("exact member filter retains all overloads", () =>
    {
        using var document = Inspect("--member", "Overload");
        var members = document.RootElement.GetProperty("types")[0].GetProperty("members");
        Expect(members.GetArrayLength() == 2, "Overloads lost or unrelated members returned");
        var signatures = members.EnumerateArray().Select(item => item.GetProperty("signature").GetString()).ToArray();
        Expect(signatures.Distinct().Count() == 2, "Overloads have identical signatures");
    });

    Check("multiple exact types and stable JSON", () =>
    {
        var arguments = new[] { "--assembly", fixture, "--type", typeName, "--type", "InspectorFixture.Other", "--json" };
        var first = Run(arguments);
        var second = Run(arguments);
        Expect(first.Code == 0 && first.Output == second.Output, first.Error);
        using var document = JsonDocument.Parse(first.Output);
        Expect(document.RootElement.GetProperty("types").GetArrayLength() == 2, "Missing requested type");
    });

    Check("human output reports signatures without executing code", () =>
    {
        var result = Run("--assembly", fixture, "--type", typeName, "--member", "Overload");
        Expect(result.Code == 0 && result.Error.Length == 0, result.Error);
        Expect(result.Output.Contains("System.Int32") && result.Output.Contains("System.String"), "Missing readable signatures");
        Expect(!File.Exists(marker), "Text inspection executed fixture code");
    });

    Check("missing type fails even after a valid type", () =>
        Fails("Missing.Type", "--assembly", fixture, "--type", typeName, "--type", "Missing.Type", "--json"));
    Check("missing member fails", () =>
        Fails("Absent", "--assembly", fixture, "--type", typeName, "--member", "Absent"));
    Check("invalid CLI arguments fail explicitly", () =>
    {
        Fails("--type", "--assembly", fixture);
        Fails("--assembly", "--type", typeName);
        Fails("--unknown", "--unknown");
        Fails("--assembly", "--assembly");
        Fails("more than once", "--assembly", fixture, "--assembly", fixture, "--type", typeName);
        Fails("more than once", "--assembly", fixture, "--type", typeName, "--json", "--json");
        Fails("--member", "--assembly", fixture, "--type", typeName, "--member", "--json");
        Fails("directory", "--assembly", fixture, "--type", typeName, "--reference", Path.Combine(temp, "absent"));
        Fails("assembly", "--assembly", Path.Combine(temp, "absent.dll"), "--type", typeName);
    });

    var isolated = Path.Combine(temp, Path.GetFileName(fixture));
    File.Copy(fixture, isolated);
    Check("missing dependency is not satisfied by the test process", () =>
        Fails("AssemblyInspector.Dependency", "--assembly", isolated, "--type", typeName, "--json"));
    Check("explicit reference directory resolves isolated assembly", () =>
    {
        var result = Run("--assembly", isolated, "--type", typeName, "--reference", Path.GetDirectoryName(dependency)!, "--json");
        Expect(result.Code == 0 && result.Error.Length == 0, result.Error);
        Expect(!File.Exists(marker), "Reference inspection executed code");
    });
    Check("identical reference copies are not ambiguous", () =>
    {
        var identical = Path.Combine(temp, "identical");
        Directory.CreateDirectory(identical);
        File.Copy(dependency, Path.Combine(identical, Path.GetFileName(dependency)));
        var result = Run("--assembly", fixture, "--type", typeName, "--reference", identical, "--reference", identical, "--json");
        Expect(result.Code == 0 && result.Error.Length == 0, result.Error);
    });
    Check("help needs no target and executes nothing", () =>
    {
        var result = Run("--help");
        Expect(result.Code == 0 && result.Error.Length == 0 && result.Output.Contains("--assembly"), "Missing usage");
        Expect(!File.Exists(marker), "Help executed fixture code");
    });
    Check("conflicting dependency candidates fail explicitly", () =>
    {
        var conflict = Path.Combine(temp, "conflict");
        Directory.CreateDirectory(conflict);
        var candidate = Path.Combine(conflict, Path.GetFileName(dependency));
        var bytes = File.ReadAllBytes(dependency).Concat(new byte[] { 1 }).ToArray();
        File.WriteAllBytes(candidate, bytes);
        Fails("conflicting", "--assembly", fixture, "--type", typeName, "--reference", conflict, "--json");
    });
    Check("invalid managed assembly fails without JSON", () =>
    {
        var invalid = Path.Combine(temp, "invalid.dll");
        File.WriteAllText(invalid, "not an assembly");
        Fails("assembly", "--assembly", invalid, "--type", typeName, "--json");
    });
}
finally
{
    Environment.SetEnvironmentVariable("ASSEMBLY_INSPECTOR_TEST_MARKER", previousMarker);
    Directory.Delete(temp, recursive: true);
}

Console.WriteLine($"Assembly inspector: {cases - failures}/{cases} checks passed");
return failures == 0 ? 0 : 1;
