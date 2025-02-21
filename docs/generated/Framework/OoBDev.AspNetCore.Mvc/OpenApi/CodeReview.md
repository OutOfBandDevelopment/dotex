As a senior software engineer / solutions architect, I'd review this code with the following considerations:

**Naming Conventions**

The namespace `OoBDev.AspNetCore.Mvc.OpenApi` is quite long. It's recommended to use shorter, more descriptive namespace names. Consider breaking it down into shorter namespaces, e.g., `OoBDev.OpenApi.Mvc`.

**Class Structure**

The `ApiPermissionsExtension` class is quite simple and does not have a clear separation of concerns. It's responsible for storing permissions and generating the Swagger/OpenAPI specification. Consider breaking it down into smaller classes:

1. `ApiPermissions`: a class that holds the `AllowAnonymous` and `Rights` properties.
2. `ApiPermissionsWriter`: a class that generates the Swagger/OpenAPI specification for permissions.

**Method Naming**

The `Write` method is responsible for generating the Swagger/OpenAPI specification. Consider renaming it to something more descriptive, like `GenerateOpenApiDefinition`.

**Avoid using `Distinct().ToList().AsReadOnly()`**

It's better to use a HashSet instead of a List and call the `Distinct()` method on it. This way, you will avoid the overhead of creating a list and then converting it to a read-only collection.

**Using `WriteOptionalCollection`**

If the `Rights` collection is not always present in the OpenAPI definition, it's better to use the `WriteOptionalCollection` method instead of `WriteCollection`. This will allow you to specify whether the collection is optional or not.

Here's the refactored code:

**ApiPermissions.cs**
```csharp
using System.Collections.Generic;

public class ApiPermissions
{
    public bool AllowAnonymous { get; }
    public IReadOnlyCollection<string> Rights { get; }

    public ApiPermissions(bool allowAnonymous, IEnumerable<string> rights)
    {
        AllowAnonymous = allowAnonymous;
        Rights = rights.Distinct().ToHashSet().ToList().AsReadOnly();
    }
}
```

**ApiPermissionsWriter.cs**
```csharp
using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;

public class ApiPermissionsWriter : IOpenApiExtension
{
    public void GenerateOpenApiDefinition(IOpenApiWriter writer, OpenApiSpecVersion specVersion, ApiPermissions permissions)
    {
        writer.WriteStartObject();
        writer.WriteProperty("anonymous", permissions.AllowAnonymous);
        writer.WriteOptionalCollection("rights", permissions.Rights, (w, v) => w.WriteValue(v));
        writer.WriteEndObject();
    }
}
```

**ApiPermissionsExtension.cs**
```csharp
using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;
using OoBDev.OpenApi.Mvc;

public class ApiPermissionsExtension : IOpenApiExtension
{
    public ApiPermissions Permissions { get; }

    public ApiPermissionsExtension(bool allowAnonymous, IEnumerable<string> rights)
    {
        Permissions = new ApiPermissions(allowAnonymous, rights);
    }

    public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        GenerateOpenApiDefinition(writer, specVersion, Permissions);
    }

    public void GenerateOpenApiDefinition(IOpenApiWriter writer, OpenApiSpecVersion specVersion, ApiPermissions permissions)
    {
        writer.WriteStartObject();
        writer.WriteProperty("anonymous", permissions.AllowAnonymous);
        writer.WriteOptionalCollection("rights", permissions.Rights, (w, v) => w.WriteValue(v));
        writer.WriteEndObject();
    }
}
```

This refactored code is more maintainable, as each class has a clear responsibility, and the methods and properties have more descriptive names. Additionally, the use of a HashSet and the `WriteOptionalCollection` method make the code more efficient and flexible.