using System.Globalization;
using System.Reflection;
using System.Text;

namespace AssemblyInspector;

internal sealed record AssemblyDescription(string Name, string Path);
internal sealed record InspectionReport(AssemblyDescription Assembly, TypeDescription[] Types, AssemblyDescription[] Dependencies);
internal sealed record GenericParameterDescription(string Name, string Attributes, string[] Constraints);
internal sealed record AttributeArgumentDescription(string Type, object? Value);
internal sealed record NamedAttributeArgumentDescription(string Name, bool IsField, AttributeArgumentDescription Argument);
internal sealed record AttributeDescription(string Type, AttributeArgumentDescription[] ConstructorArguments, NamedAttributeArgumentDescription[] NamedArguments);
internal sealed record ParameterDescription(string? Name, string Type, bool IsIn, bool IsOut, bool IsOptional, AttributeDescription[] Attributes);
internal sealed record AccessorDescription(string Name, string Visibility, bool IsStatic);
internal sealed record MemberDescription(
    string Kind, string Name, string Signature, string Visibility, bool IsStatic, string? Type,
    bool IsReadOnly, bool IsLiteral, ParameterDescription[] Parameters,
    GenericParameterDescription[] GenericParameters, AccessorDescription[] Accessors, AttributeDescription[] Attributes);
internal sealed record TypeDescription(
    string Name, string Kind, string Visibility, string? BaseType, string[] Interfaces,
    GenericParameterDescription[] GenericParameters, AttributeDescription[] Attributes, MemberDescription[] Members);

internal static class Inspector
{
    private const BindingFlags Declared = BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    public static InspectionReport Inspect(string assemblyPath, List<string> typeNames, string? memberName, List<string> references)
    {
        assemblyPath = Path.GetFullPath(assemblyPath);
        if (!File.Exists(assemblyPath)) throw new FileNotFoundException($"Assembly file does not exist: {assemblyPath}");
        var resolver = new MetadataResolver(assemblyPath, references);
        using var context = new MetadataLoadContext(resolver, resolver.CoreAssemblyName);
        var assembly = context.LoadFromAssemblyPath(assemblyPath);
        var types = typeNames.Distinct(StringComparer.Ordinal).Select(name =>
        {
            var type = assembly.GetType(name, throwOnError: true, ignoreCase: false)!;
            var members = type.GetMembers(Declared)
                .Where(item => item is FieldInfo or PropertyInfo or MethodBase or EventInfo)
                .Where(item => memberName is null || item.Name == memberName)
                .Select(DescribeMember)
                .OrderBy(item => item.Kind, StringComparer.Ordinal)
                .ThenBy(item => item.Signature, StringComparer.Ordinal)
                .ToArray();
            if (memberName is not null && members.Length == 0)
                throw new ArgumentException($"Member '{memberName}' was not declared on type '{name}'.");
            return new TypeDescription(
                type.FullName ?? type.Name,
                type.IsInterface ? "interface" : type.IsEnum ? "enum" : type.IsValueType ? "struct" : "class",
                Visibility(type), type.BaseType is null ? null : FormatType(type.BaseType),
                type.GetInterfaces().Select(FormatType).Order(StringComparer.Ordinal).ToArray(),
                GenericParameters(type.IsGenericTypeDefinition ? type.GetGenericArguments() : []),
                Attributes(type.GetCustomAttributesData()), members);
        }).ToArray();
        return new InspectionReport(
            new AssemblyDescription(assembly.FullName!, assemblyPath), types,
            context.GetAssemblies().Where(item => item != assembly)
                .Select(item => new AssemblyDescription(item.FullName!, item.Location))
                .OrderBy(item => item.Name, StringComparer.Ordinal).ToArray());
    }

    private static MemberDescription DescribeMember(MemberInfo member)
    {
        var attributes = Attributes(member.GetCustomAttributesData());
        if (member is FieldInfo field)
            return new("field", field.Name, $"{Visibility(field)}{(field.IsStatic ? " static" : "")} {FormatType(field.FieldType)} {field.Name}",
                Visibility(field), field.IsStatic, FormatType(field.FieldType), field.IsInitOnly, field.IsLiteral, [], [], [], attributes);

        if (member is MethodBase method)
        {
            var parameters = method.GetParameters().Select(DescribeParameter).ToArray();
            var generic = GenericParameters(method.IsGenericMethodDefinition ? method.GetGenericArguments() : []);
            var returnType = method is MethodInfo info ? FormatType(info.ReturnType) : null;
            var signature = $"{Visibility(method)}{(method.IsStatic ? " static" : "")} {returnType} {method.Name}"
                + (generic.Length == 0 ? "" : "<" + string.Join(", ", generic.Select(item => item.Name)) + ">")
                + "(" + string.Join(", ", parameters.Select(item => $"{(item.IsOut ? "out " : item.IsIn ? "in " : "")}{item.Type} {item.Name}")) + ")";
            return new(method is ConstructorInfo ? "constructor" : "method", method.Name, signature.Trim(),
                Visibility(method), method.IsStatic, returnType, false, false, parameters, generic, [], attributes);
        }

        MethodInfo[] methods;
        string kind;
        string valueType;
        ParameterDescription[] indexes;
        if (member is PropertyInfo property)
        {
            methods = property.GetAccessors(nonPublic: true);
            kind = "property";
            valueType = FormatType(property.PropertyType);
            indexes = property.GetIndexParameters().Select(DescribeParameter).ToArray();
        }
        else if (member is EventInfo eventInfo)
        {
            methods = new[] { eventInfo.GetAddMethod(true), eventInfo.GetRemoveMethod(true), eventInfo.GetRaiseMethod(true) }
                .OfType<MethodInfo>().ToArray();
            kind = "event";
            valueType = FormatType(eventInfo.EventHandlerType!);
            indexes = [];
        }
        else throw new NotSupportedException($"Unsupported member: {member.MemberType}.");

        var accessors = methods.Select(item => new AccessorDescription(item.Name, Visibility(item), item.IsStatic)).ToArray();
        var visibility = methods.Select(Visibility).OrderBy(VisibilityRank).First();
        var isStatic = methods.Any(item => item.IsStatic);
        var indexedName = member.Name + (indexes.Length == 0 ? "" : "[" + string.Join(", ", indexes.Select(item => item.Type)) + "]");
        return new(kind, member.Name, $"{visibility}{(isStatic ? " static" : "")} {valueType} {indexedName}",
            visibility, isStatic, valueType, false, false, indexes, [], accessors, attributes);
    }

    private static ParameterDescription DescribeParameter(ParameterInfo parameter)
        => new(parameter.Name, FormatType(parameter.ParameterType), parameter.IsIn, parameter.IsOut,
            parameter.IsOptional, Attributes(parameter.GetCustomAttributesData()));

    private static GenericParameterDescription[] GenericParameters(Type[] parameters)
        => parameters.Select(item => new GenericParameterDescription(item.Name, item.GenericParameterAttributes.ToString(),
            item.GetGenericParameterConstraints().Select(FormatType).ToArray())).ToArray();

    private static AttributeDescription[] Attributes(IList<CustomAttributeData> attributes)
        => attributes.Select(item => new AttributeDescription(FormatType(item.AttributeType),
            item.ConstructorArguments.Select(AttributeArgument).ToArray(),
            item.NamedArguments.Select(argument => new NamedAttributeArgumentDescription(argument.MemberName,
                argument.IsField, AttributeArgument(argument.TypedValue))).ToArray())).ToArray();

    private static AttributeArgumentDescription AttributeArgument(CustomAttributeTypedArgument argument)
    {
        object? value = argument.Value switch
        {
            Type type => FormatType(type),
            IList<CustomAttributeTypedArgument> items => items.Select(AttributeArgument).ToArray(),
            _ => argument.Value
        };
        return new(FormatType(argument.ArgumentType), value);
    }

    private static string FormatType(Type type)
    {
        if (type.IsGenericParameter) return type.Name;
        if (type.IsByRef) return FormatType(type.GetElementType()!) + "&";
        if (type.IsPointer) return FormatType(type.GetElementType()!) + "*";
        if (type.IsArray) return FormatType(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
        var name = type.FullName ?? type.Name;
        if (!type.IsGenericType) return name;
        name = type.GetGenericTypeDefinition().FullName ?? type.Name;
        var simpleName = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            if (name[i] != '`') simpleName.Append(name[i]);
            else while (i + 1 < name.Length && char.IsAsciiDigit(name[i + 1])) i++;
        }
        return simpleName + "<" + string.Join(", ", type.GetGenericArguments().Select(FormatType)) + ">";
    }

    private static string Visibility(Type type) => type.IsNested
        ? type.IsNestedPublic ? "public" : type.IsNestedPrivate ? "private" : type.IsNestedFamily ? "protected"
        : type.IsNestedFamORAssem ? "protected internal" : type.IsNestedFamANDAssem ? "private protected" : "internal"
        : type.IsPublic ? "public" : "internal";

    private static string Visibility(FieldInfo field) => field.IsPublic ? "public" : field.IsPrivate ? "private"
        : field.IsFamily ? "protected" : field.IsFamilyOrAssembly ? "protected internal"
        : field.IsFamilyAndAssembly ? "private protected" : "internal";

    private static string Visibility(MethodBase method) => method.IsPublic ? "public" : method.IsPrivate ? "private"
        : method.IsFamily ? "protected" : method.IsFamilyOrAssembly ? "protected internal"
        : method.IsFamilyAndAssembly ? "private protected" : "internal";

    private static int VisibilityRank(string visibility) => visibility switch
    {
        "public" => 0, "protected internal" => 1, "protected" => 2, "internal" => 3, "private protected" => 4, _ => 5
    };

    public static string ToText(InspectionReport report)
    {
        var text = new StringBuilder().AppendLine($"Assembly: {report.Assembly.Name}").AppendLine($"Path: {report.Assembly.Path}");
        foreach (var type in report.Types)
        {
            text.AppendLine().AppendLine($"{type.Visibility} {type.Kind} {type.Name}");
            if (type.BaseType is not null) text.AppendLine($"  Base: {type.BaseType}");
            foreach (var item in type.Interfaces) text.AppendLine($"  Interface: {item}");
            foreach (var parameter in type.GenericParameters) text.AppendLine($"  Generic: {parameter.Name} ({parameter.Attributes}; {string.Join(", ", parameter.Constraints)})");
            AppendAttributes(text, type.Attributes, "  ");
            foreach (var member in type.Members)
            {
                text.AppendLine($"  {member.Kind}: {member.Signature}");
                foreach (var accessor in member.Accessors) text.AppendLine($"    Accessor: {accessor.Visibility}{(accessor.IsStatic ? " static" : "")} {accessor.Name}");
                foreach (var parameter in member.GenericParameters) text.AppendLine($"    Generic: {parameter.Name} ({parameter.Attributes}; {string.Join(", ", parameter.Constraints)})");
                AppendAttributes(text, member.Attributes, "    ");
            }
        }
        text.AppendLine().AppendLine("Resolved metadata dependencies:");
        foreach (var dependency in report.Dependencies) text.AppendLine($"  {dependency.Name}: {dependency.Path}");
        return text.ToString().TrimEnd();
    }

    private static void AppendAttributes(StringBuilder text, AttributeDescription[] attributes, string indent)
    {
        foreach (var attribute in attributes)
        {
            var arguments = attribute.ConstructorArguments.Select(FormatArgument)
                .Concat(attribute.NamedArguments.Select(item => $"{item.Name} = {FormatArgument(item.Argument)}"));
            text.AppendLine($"{indent}Attribute: {attribute.Type}({string.Join(", ", arguments)})");
        }
    }

    private static string FormatArgument(AttributeArgumentDescription argument) => argument.Value switch
    {
        null => "null",
        AttributeArgumentDescription[] items => "[" + string.Join(", ", items.Select(FormatArgument)) + "]",
        string value => System.Text.Json.JsonSerializer.Serialize(value),
        _ => Convert.ToString(argument.Value, CultureInfo.InvariantCulture)!
    };
}
