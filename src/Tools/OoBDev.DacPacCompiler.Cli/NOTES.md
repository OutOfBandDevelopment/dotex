Yes — **you can (and should) switch to the 4.8.1 SQLCLR assembly**, and you do **not** need the netstandard2.1 shim at all **if you move to metadata-only inspection**.

What’s biting you right now is that the **netstandard2.1 version exists only to make the assembly loadable on CoreCLR**, which forces compromises and subtle behavioral differences. Since you are **not executing** the SQLCLR code, that workaround is unnecessary.

Below is the **clean, correct migration path**.

---

## Target architecture (recommended)

```
DacPacBuilder        (net9.0)
│
├─ build dependency → SqlClrProject (net481)
│                    (NO runtime reference)
│
└─ loads via MetadataLoadContext (metadata only)
```

You will:

* **Delete the netstandard2.1 SQLCLR build**
* Keep **one authoritative SQLCLR assembly (net481)**
* Load it **reflection-only** from net9

---

## Step 1: Remove the netstandard dependency

### In the net9.0 `.csproj`

```xml
<ItemGroup>
  <ProjectReference Include="..\SqlClrProject\SqlClrProject.csproj"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

❌ No `<Reference>`
❌ No `<PackageReference>`
❌ No netstandard build

This is now a **build-order + artifact dependency only**.

---

## Step 2: Switch to `MetadataLoadContext`

Replace this:

```csharp
var sqlClrAssembly = Assembly.LoadFile(assemblyFileNet);
```

With this **canonical loader**:

```csharp
using System.Reflection;
using System.Reflection.MetadataLoadContext;
using System.Runtime.InteropServices;

static Assembly LoadForInspection(string assemblyPath)
{
    var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();

    var paths = Directory.GetFiles(runtimeDir, "*.dll")
        .Concat(new[]
        {
            assemblyPath,
            Path.Combine(Path.GetDirectoryName(assemblyPath)!, "System.Data.dll"),
            Path.Combine(Path.GetDirectoryName(assemblyPath)!, "Microsoft.SqlServer.Types.dll"),
        })
        .Where(File.Exists)
        .Distinct();

    var resolver = new PathAssemblyResolver(paths);
    var mlc = new MetadataLoadContext(resolver);

    return mlc.LoadFromAssemblyPath(assemblyPath);
}
```

Usage:

```csharp
using var mlc = LoadForInspection(assemblyFileFramework);
var sqlClrAssembly = mlc;
```

(You may want to return `(MetadataLoadContext, Assembly)` instead.)

---

## Step 3: Fix type comparisons (critical)

You **must stop using runtime `Type` identity**.

### ❌ This must go

```csharp
typeof(SqlString)
typeof(IEnumerable)
_isBuiltIn.Contains(type)
```

These will **never match** metadata types.

---

### ✅ Use `FullName` comparisons instead

#### Example: IEnumerable detection

```csharp
bool IsEnumerable(Type t) =>
    t.GetInterfaces().Any(i =>
        i.FullName == "System.Collections.IEnumerable");
```

---

#### Example: SQL type mapping

Replace:

```csharp
Dictionary<Type, string>
```

With:

```csharp
Dictionary<string, string>
```

```csharp
private static readonly IReadOnlyDictionary<string, string> _typeName =
    new Dictionary<string, string>
{
    ["System.Data.SqlTypes.SqlByte"] = "[tinyint]",
    ["System.Data.SqlTypes.SqlInt16"] = "[smallint]",
    ["System.Data.SqlTypes.SqlInt32"] = "[int]",
    ["System.Data.SqlTypes.SqlInt64"] = "[bigint]",
    ["System.Data.SqlTypes.SqlString"] = "[nvarchar]",
    ["Microsoft.SqlServer.Types.SqlGeography"] = "[geography]",
    ["System.Guid"] = "[uniqueidentifier]",
};
```

Then:

```csharp
private string GetTypeName(Type type) =>
    type.FullName != null && _typeName.TryGetValue(type.FullName, out var name)
        ? name
        : throw new NotSupportedException($"No mapping for {type.FullName}");
```

---

## Step 4: Attributes work as-is

This **continues to work unchanged**:

```csharp
type.GetCustomAttributes<SqlFunctionAttribute>()
```

Why?

* Attribute data is stored in metadata
* No code execution
* Fully supported in `MetadataLoadContext`

---

## Step 5: Delete the netstandard2.1 build

Once you validate:

* Aggregates discovered
* Functions discovered
* UDTs discovered
* DACPAC identical

You can **remove the netstandard target entirely**.

---

## Why this is the *right* fix

| Problem              | Old approach     | New approach  |
| -------------------- | ---------------- | ------------- |
| SQLCLR compatibility | netstandard shim | native net481 |
| Runtime safety       | risky            | safe          |
| Future-proof         | ❌                | ✅             |
| DACPAC fidelity      | compromised      | exact         |

This is **exactly** how Roslyn, ILLink, and SDK tooling inspect assemblies.

---

## Final answer (short)

> **Yes, you can switch to the 4.8.1 assembly.**
> Use `MetadataLoadContext`, remove runtime references, and compare types by name.

If you want, I can:

* Patch your full class and return a drop-in version
* Add unit tests validating both assemblies
* Help wire this into MSBuild / CI

You’re on the right track — this is a solid design.
