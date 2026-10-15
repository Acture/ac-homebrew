using InspectorDependency;

namespace InspectorFixture;

public static class Hazard
{
    public static void Execute(string operation)
    {
        var marker = Environment.GetEnvironmentVariable("ASSEMBLY_INSPECTOR_TEST_MARKER");
        if (marker is not null) File.AppendAllText(marker, operation + Environment.NewLine);
        throw new InvalidOperationException("Fixture code executed: " + operation);
    }
}

[AttributeUsage(AttributeTargets.All)]
public sealed class DangerousAttribute : Attribute
{
    public DangerousAttribute(string label, Type relatedType, params int[] values)
        => Hazard.Execute("attribute constructor");
    public string Note { get; set; } = "";
}

[Dangerous("type", typeof(ExternalValue), 2, 4, Note = "metadata only")]
public class Dangerous<T> : ExternalBase where T : class, new()
{
    static Dangerous() => Hazard.Execute("static initializer");
    public Dangerous() => Hazard.Execute("constructor");
    [Dangerous("field", typeof(string))]
    private static readonly int hidden = 7;
    public const int Constant = 42;
    public ExternalValue? Value;
    public int Explosive
    {
        get { Hazard.Execute("getter"); return hidden; }
        private set => Hazard.Execute("setter");
    }
    public event Action? Changed
    {
        add => Hazard.Execute("event add");
        remove => Hazard.Execute("event remove");
    }
    public void Overload(int value) => Hazard.Execute("method");
    public void Overload(string value) => Hazard.Execute("method");
    private static U Convert<U>(ref T input, out int count) where U : struct
    {
        count = 0;
        Hazard.Execute("generic method");
        return default;
    }
}

public class Other { }
