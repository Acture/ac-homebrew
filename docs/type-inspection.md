# Read-only assembly and type inspection

Use the contributor tool in `scripts/AssemblyInspector` to examine managed DLL
metadata without loading their code into an execution context. It requires the
.NET 10 SDK. The tool and its reports are not part of the player ZIP.

```powershell
$managed = "<game directory>\Wrath_Data\Managed"
dotnet run --project scripts\AssemblyInspector --configuration Release -- `
  --assembly "$managed\Assembly-CSharp.dll" `
  --type Kingmaker.Localization.LocalizedString `
  --type Kingmaker.Localization.LocalizationManager
```

`--type` is an exact, case-sensitive metadata name and can be repeated. Nested
types use `Outer+Inner`; generic definitions retain their arity suffix, such as
``Namespace.Container`1``. By default the report includes all declared public and
non-public, static and instance fields, properties, events, constructors and
methods. Inherited members are excluded; base types and interfaces are reported
separately.

To restrict members, add `--member CurrentLocale`. This is an exact member name,
not a substring or regular expression, and includes all matching overloads.
Property and event accessors retain their individual accessibility. Generic
constraints, parameter types and custom-attribute arguments are metadata too.
No field or property values are read.

## Dependencies and machine-readable output

The target DLL's directory supplies managed dependency candidates. Add
`--reference "<directory>"` for each additional dependency directory; discovery
is not recursive. Native DLLs in those directories are ignored. For example:

```powershell
dotnet run --project scripts\AssemblyInspector --configuration Release -- `
  --assembly "artifacts\bin\ACHomebrew\Release\ACHomebrew.dll" `
  --type ACHomebrew.Feats.FeatTextCatalog `
  --reference "$managed" `
  --json > artifacts\type-inspection.json
```

Use the actual build output path if it differs from the example. The host .NET
runtime directory is a fallback for framework metadata, not for application or
game dependencies. When supplied directories contain `mscorlib`, that assembly
is the metadata core library; otherwise the host's `System.Private.CoreLib` is
used. Dependency binding is by simple assembly name. Different identities or
bytes for a needed name in supplied directories are an error; byte-identical
copies are accepted. Host framework fallback is not evidence of runtime
compatibility.

Some installations contain different Harmony DLLs in the main managed directory
and the Unity Mod Manager subdirectory. Supplying both directories for a type
that needs Harmony will report that conflict rather than silently choose one.
For such cases, prepare an ignored directory containing only the intended
reference DLLs, or inspect a type that does not require the ambiguous dependency.

`--json` emits a single object with:

| Property | Contents |
|---|---|
| `assembly` | Target assembly identity and absolute path |
| `types` | Requested type names, kind, visibility, base, interfaces, generic constraints, attributes and declared members |
| `types[].members` | Kind, name, signature, visibility, static/read-only/literal flags, type, parameters, generic constraints, accessors and attributes |
| `dependencies` | Identities and absolute paths of metadata assemblies actually loaded for the report |

Attributes contain their type, typed constructor arguments and typed named
arguments. Array arguments remain arrays; type arguments are metadata type-name
strings. This describes encoded attributes, not instantiated attribute objects.
Reports are deterministic for the same inputs and paths. They include local
absolute paths; review reports before sharing them.

Invalid options, absent types or members, missing dependencies and conflicting
dependency candidates produce a diagnostic on stderr and a nonzero exit code.
The tool builds the entire requested report before writing stdout, so a later
failure does not emit an earlier type as a successful partial JSON report.

## Safety and validation limits

Inspection uses `System.Reflection.MetadataLoadContext`. It does not instantiate
target objects, invoke methods or property getters, read live fields, execute
static initializers, or call custom-attribute constructors. Attribute data is
read with `GetCustomAttributesData`, not `GetCustomAttributes`.

Run the game-independent safety and failure controls with:

```powershell
dotnet run --project tests\AssemblyInspector.Tests --configuration Release
```

The fixture has deliberately hazardous static initialization, constructors,
methods, accessors and attribute constructors. Executing them writes a marker
and throws; successful inspection must leave that marker absent. Tests also
check generic/private/static metadata, overloaded member filtering, structured
and readable output, missing dependency isolation and conflicting dependencies.
The same suite runs in public CI without any licensed game files.

Metadata inspection can establish that a type or member exists in a particular
DLL and how its signature is declared. It cannot establish Unity native-call
availability, Mono behavior, getter results, localization refresh, game
initialization, combat behavior or ABI compatibility. Keep its reports separate
from runtime and game integration evidence. Do not commit game DLLs or generated
reports.
