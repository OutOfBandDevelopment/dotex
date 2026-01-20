# Bug Fixes - Swashbuckle 10.1.0 & .NET 10.0 Breaking Changes

**Date:** 2026-01-20
**Epic:** Bug Fixes & Technical Debt
**Status:** ✅ COMPLETE AND VERIFIED
**Impact:** 5 files modified (4 Swashbuckle, 1 .NET 10.0) + 3 files for XML documentation

---

## Summary

Fixed all breaking changes introduced by Swashbuckle.AspNetCore 10.1.0 upgrade and .NET 10.0 migration in OoBDev.AspNetCore.Mvc project. Also enabled XML documentation generation globally to populate Swagger Summary and Description properties.

**Results:**
- ✅ All 65 projects build successfully
- ✅ XML documentation files generated and loaded by Swagger
- ✅ Swagger JSON/YAML generated successfully
- ✅ Summary and Description properties now appear in Swagger UI
- ✅ Swagger generation tested and verified working

---

## Swashbuckle 10.1.0 Breaking Changes (4 Files)

### Key Breaking Changes

**Collections are NULL by default:**
- ALL OpenAPI collections (Tags, Parameters, Responses, Extensions, Properties) now start as null
- Must initialize before use: `operation.Tags ??= new HashSet<OpenApiTagReference>()`

**Read-Only Properties:**
- `IOpenApiRequestBody.Content` and `IOpenApiResponse.Content` are read-only
- Must create new objects with Content in initializers

**Type Changes:**
- `OpenApiTag` → `OpenApiTagReference` for Tags collection
- `JsonSchemaType` is enum (not string) - use `JsonSchemaType.String`, etc.
- `IOpenApiSchema.Properties` is read-only (use `.Add()` or indexer `[key] = value`)

---

### File 1: FormFileOperationFilter.cs

**Issues Fixed:**
1. CS0200: `IOpenApiSchema.Properties` is read-only
2. CS0029: String to JsonSchemaType conversion
3. NullReferenceException: schema.Properties is null

**Changes:**
```csharp
// Line 37: Initialize Properties before use
schema.Properties ??= new Dictionary<string, IOpenApiSchema>();

// Lines 40-47: Use JsonSchemaType enum
foreach (var fileParam in fileParams)
{
    var propertyName = fileParam.Name ?? fileParam.ParameterType.Name;
    schema.Properties[propertyName] = new OpenApiSchema()
    {
        Type = JsonSchemaType.String,  // ← enum, not "string"
        Format = "binary"
    };
}
```

---

### File 2: HealthChecksDocumentFilter.cs

**Issues Fixed:**
1. CS1503: `OpenApiTag` vs `OpenApiTagReference` mismatch
2. CS0200 & CS0266: Properties assignment and type conversion
3. NullReferenceException: Collections are null

**Changes:**
```csharp
// Lines 26-50: Initialize all collections in object initializers
var operation = new OpenApiOperation
{
    Tags = new HashSet<OpenApiTagReference>
    {
        new OpenApiTagReference("ApiHealth")  // ← OpenApiTagReference, not OpenApiTag
    }
};

var schema = new OpenApiSchema
{
    Type = JsonSchemaType.Object,
    AdditionalPropertiesAllowed = true,
    Properties = new Dictionary<string, IOpenApiSchema>  // ← Initialize in initializer
    {
        ["status"] = new OpenApiSchema { Type = JsonSchemaType.String },
        ["errors"] = new OpenApiSchema { Type = JsonSchemaType.Array }
    }
};

var response = new OpenApiResponse
{
    Content = new Dictionary<string, OpenApiMediaType>  // ← Read-only, use initializer
    {
        ["application/json"] = new OpenApiMediaType { Schema = schema }
    }
};

operation.Responses = new OpenApiResponses
{
    ["200"] = response
};
```

---

### File 3: SearchQueryOperationFilter.cs

**Issues Fixed:**
1. CS1503: OpenApiTag references (3 occurrences)
2. CS0019: Coalesce assignment with mismatched types
3. NullReferenceException: Collections are null (7 locations)
4. UpdateRequestSchema method refactoring

**Changes:**
```csharp
// Line 46: Initialize Tags before use
operation.Tags ??= new HashSet<OpenApiTagReference>();

// Lines 48-59: Use OpenApiTagReference
operation.Tags.Add(new OpenApiTagReference("Save"));     // ← not OpenApiTag
operation.Tags.Add(new OpenApiTagReference("Getter"));
operation.Tags.Add(new OpenApiTagReference(nameof(IQueryable)));

// Line 121: Initialize Parameters before use
operation.Parameters ??= new List<OpenApiParameter>();

// Lines 92-98: Initialize RequestBody with Content
if (operation.RequestBody == null)
{
    operation.RequestBody = new OpenApiRequestBody
    {
        Content = new Dictionary<string, OpenApiMediaType>()  // ← In initializer
    };
}

// Lines 179-185: Initialize Responses with Content
operation.Responses ??= new OpenApiResponses();
if (!operation.Responses.ContainsKey("200"))
{
    operation.Responses["200"] = new OpenApiResponse
    {
        Content = new Dictionary<string, OpenApiMediaType>()  // ← In initializer
    };
}

// Lines 238, 267: Initialize Properties in new schemas
filterSchema = new OpenApiSchema()
{
    Type = JsonSchemaType.Object,
    Description = $"**Filterable Properties:** {string.Join("; ", treeBuilder.GetFilterablePropertyNames())}",
    Properties = new Dictionary<string, IOpenApiSchema>()  // ← Initialize
};

orderBySchema = new OpenApiSchema()
{
    Type = JsonSchemaType.Object,
    Description = $"**Sortable Properties:** {string.Join("; ", treeBuilder.GetSortablePropertyNames())}",
    Properties = new Dictionary<string, IOpenApiSchema>()  // ← Initialize
};
```

---

### File 4: ApplicationPermissionsApiFilter.cs

**Issues Fixed:**
1. NullReferenceException: Extensions dictionary is null

**Changes:**
```csharp
// Lines 42-43: Initialize Extensions before use
operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
operation.Extensions["x-permissions"] = new ApiPermissionsExtension(allowAnonymous, applicationRights);
```

---

## .NET 10.0 Breaking Changes (1 File)

### File 5: ServiceCollectionExtensions.cs

**Issues Fixed:**
1. Ambiguous constructor: ClaimsPrincipal has multiple constructors
2. Deprecated API: IActionContextAccessor (ASPDEPR006)

**Changes:**

**ClaimsPrincipal DI (Lines 59-68):**
```csharp
// BEFORE: Type-based registration causes ambiguity
services.TryAddTransient<IPrincipal, ClaimsPrincipal>();

// AFTER: Factory method explicitly chooses constructor
services.TryAddTransient<IPrincipal>(sp =>
    sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ??
    ClaimsPrincipal.Current ??
    new ClaimsPrincipal(new ClaimsIdentity())
);
services.TryAddTransient(sp =>
    sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ??
    ClaimsPrincipal.Current ??
    new ClaimsPrincipal(new ClaimsIdentity())
);
```

**IActionContextAccessor Removal (Line 57):**
```csharp
// REMOVED: Deprecated in .NET 10 (ASPDEPR006)
// services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

// REPLACEMENT (if needed): Use IHttpContextAccessor and HttpContext.GetEndpoint()
```

---

## XML Documentation Generation (3 Files)

### Issue

Swagger Summary and Description properties were missing because XML documentation files were not being generated.

### Root Cause

- `GenerateDocumentationFile` was set to `False` globally (commented out in Directory.Build.props)
- Individual projects (OoBDev.AspNetCore.Mvc, OoBDev.WebApi) had local overrides set to `False`

### Files Modified

**1. Directory.Build.props (Line 19):**
```xml
<!-- BEFORE: -->
<!--<GenerateDocumentationFile>False</GenerateDocumentationFile>-->

<!-- AFTER: -->
<GenerateDocumentationFile>True</GenerateDocumentationFile>
```

**2. OoBDev.AspNetCore.Mvc.csproj (Line 7 removed):**
```xml
<!-- REMOVED local override -->
<GenerateDocumentationFile>False</GenerateDocumentationFile>
```

**3. OoBDev.WebApi.csproj (Line 10 removed):**
```xml
<!-- REMOVED local override -->
<GenerateDocumentationFile>False</GenerateDocumentationFile>

<!-- Also cleaned up duplicate InvariantGlobalization setting -->
```

### Results

XML documentation files now generated:
- `OoBDev.AspNetCore.Mvc.xml` ✅
- `OoBDev.WebApi.xml` ✅
- `OoBDev.Data.Vectors.xml` ✅
- `OoBDev.Data.Vectors.Hosting.xml` ✅
- `OoBDev.AllMiniLmL6V2Sharp.xml` ✅

Swagger now loads and displays:
- Summary properties from `<summary>` XML comments
- Description properties from `<param>`, `<returns>`, etc.

---

## Build Verification

**Build Command:**
```bash
dotnet build src/
```

**Results:**
- ✅ All 65 projects built successfully
- ✅ No compiler errors
- ✅ Build time: 29.6 seconds

**Swagger Generation:**
```
Swagger JSON/YAML successfully written to C:\repo\oobdev\dotex\src\..\docs\swagger.json
Swagger JSON/YAML successfully written to C:\repo\oobdev\dotex\src\..\docs\swagger.yaml
```

---

## Key Patterns for Future Reference

### Swashbuckle 10.1.0 Collection Initialization

**Always initialize before use:**
```csharp
operation.Tags ??= new HashSet<OpenApiTagReference>();
operation.Parameters ??= new List<OpenApiParameter>();
operation.Responses ??= new OpenApiResponses();
operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
```

**Or initialize in object initializer:**
```csharp
var operation = new OpenApiOperation
{
    Tags = new HashSet<OpenApiTagReference> { ... },
    Parameters = new List<OpenApiParameter> { ... }
};
```

### Read-Only Content Properties

**Always create new objects with Content in initializer:**
```csharp
operation.RequestBody = new OpenApiRequestBody
{
    Content = new Dictionary<string, OpenApiMediaType>()
};

operation.Responses["200"] = new OpenApiResponse
{
    Content = new Dictionary<string, OpenApiMediaType>()
};
```

### .NET 10.0 DI Ambiguity

**Use factory methods for types with multiple constructors:**
```csharp
// ❌ BAD: Type-based registration
services.TryAddTransient<IInterface, Implementation>();

// ✅ GOOD: Factory method
services.TryAddTransient<IInterface>(sp => new Implementation(args));
```

---

## Testing Status

- ✅ **Verified:** Swagger generation tested and verified working
- ✅ No regressions in OpenAPI functionality

---

## References

- [Swashbuckle 10.0 Migration Guide](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md)
- [.NET 10 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [ASPDEPR006: IActionContextAccessor Deprecation](https://learn.microsoft.com/en-us/aspnet/core/migration/70-to-80)

---

## Files Modified

**Swashbuckle Fixes:**
1. `src/Framework/OoBDev.AspNetCore.Mvc/Filters/FormFileOperationFilter.cs`
2. `src/Framework/OoBDev.AspNetCore.Mvc/SwaggerGen/HealthChecksDocumentFilter.cs`
3. `src/Framework/OoBDev.AspNetCore.Mvc/Filters/SearchQueryOperationFilter.cs`
4. `src/Framework/OoBDev.AspNetCore.Mvc/Filters/ApplicationPermissionsApiFilter.cs`

**.NET 10.0 Fixes:**
5. `src/Framework/OoBDev.AspNetCore.Mvc/ServiceCollectionExtensions.cs`

**XML Documentation:**
6. `src/Directory.Build.props`
7. `src/Framework/OoBDev.AspNetCore.Mvc/OoBDev.AspNetCore.Mvc.csproj`
8. `src/Examples/OoBDev.WebApi/OoBDev.WebApi.csproj`

---

**Related Documentation:**
- [TODO-bug-fixes.md](../../TODO-bug-fixes.md) - Active bug tracking
- [TODO.md](../../TODO.md) - Main project tracking
