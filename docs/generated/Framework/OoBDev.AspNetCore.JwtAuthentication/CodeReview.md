As a senior software engineer and solutions architect, I've reviewed the provided code and identified areas for improvement to make it more maintainable. Here's a detailed breakdown of my suggestions:

**1. Code organization and structure**

The code is quite dense, and it's difficult to understand the overall architecture. I recommend breaking down the solution into smaller, more focused projects or modules. This will make it easier to manage and maintain the codebase.

**2. Naming conventions**

Some namespace and class names don't follow the conventional naming guidelines. For example, `OoBDev.AspNetCore.JwtAuthentication.SwaggerGen` is quite long and doesn't follow the standard .NET namespace convention (e.g., `OoBDev.AspNetCore.JwtAuthentication.Swagger`). Consider renaming these namespaces and classes to follow a more consistent naming scheme.

**3. Use of `record` keyword**

The `JwtExtensionBuilder` class uses the `record` keyword to define its constructor. While this is a good practice for immutable objects, it might not be immediately clear to some developers what the `record` keyword does. Consider adding a comment or documentation to explain its purpose.

**4. Type handling**

The code uses `string` types for configuration section names and default schema values. Consider using `static class` or `enum` types to define these values, making the code more self-documenting and reducing the risk of typo errors.

**5. Method names and overloading**

Some method names are quite long and might be prone to typos. Consider renaming them to be more concise and descriptive. Also, there is no clear indication of which overload to use when calling the `TryAddJwtBearerServices` method. Consider adding overloads with default values to make the API more user-friendly.

**6. Configuration handling**

Configuration handling is complex, and it's difficult to understand how the configuration is being set and updated. Consider breaking down configuration into smaller, more focused sections, and use more descriptive variable names to make the code easier to follow.

**7. SwaggerGen and SwaggerUI configuration**

The SwaggerGen and SwaggerUI configurations are complex and might be difficult to understand and debug. Consider breaking down the configuration into smaller components, and use more descriptive variable names to make the code easier to follow.

**8. Code duplication**

There is some code duplication between the `TryAddJwtBearerAuthentication` and `TryAddJwtBearerSwaggerGen` methods. Consider extracting common code into separate methods or classes to reduce duplication and improve maintainability.

**9. Error handling**

Error handling is minimal, and it's recommended to include more robust error handling to ensure that the application is more resilient to unexpected errors.

**10. Documentation**

The code includes some documentation comments, but they are not consistently formatted or detailed enough. Consider using a documentation tool like XML comments or Doxygen to create a comprehensive documentation for the code.

Here is an example of how some of these changes could be implemented:

**Updated namespace**

```csharp
namespace OoBDev.AspNetCore.JwtAuthentication.Swagger
{
}
```

**Updated `JwtExtensionBuilder` class**

```csharp
public static class JwtConfiguration
{
    public const string DefaultSchema = JwtBearerDefaults.AuthenticationScheme;
    public const string JwtBearerConfigurationSection = nameof(JwtBearerOptions);
    public const string OAuth2SwaggerConfigurationSection = nameof(OAuth2SwaggerOptions);
}

public record JwtExtensionBuilder
{
    public string DefaultSchema { get; init; } = JwtConfiguration.DefaultSchema;
    public string JwtBearerConfigurationSection { get; init; } = JwtConfiguration.JwtBearerConfigurationSection;
    public string OAuth2SwaggerConfigurationSection { get; init; } = JwtConfiguration.OAuth2SwaggerConfigurationSection;
}
```

**Updated `ServiceCollectionExtensions` class**

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddJwtBearerServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string defaultScheme = JwtConfiguration.DefaultSchema,
        string configurationSection = JwtConfiguration.JwtBearerConfigurationSection)
    {
        services
            .AddAuthentication(defaultScheme)
            .AddJwtBearer(options => configuration.Bind(configurationSection, options));
        // ...
    }

    public static IServiceCollection TryAddJwtBearerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string defaultScheme = JwtConfiguration.DefaultSchema,
        string configurationSection = JwtConfiguration.JwtBearerConfigurationSection)
    {
        services.Configure<JwtBearerOptions>(options => configuration.Bind(configurationSection, options));
        // ...
    }

    public static IServiceCollection TryAddJwtBearerSwaggerGen(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSection = JwtConfiguration.OAuth2SwaggerConfigurationSection)
    {
        services
            .Configure<OAuth2SwaggerOptions>(options => configuration.Bind(configurationSection, options));
        // ...
    }
}
```

These changes aim to improve the code's maintainability, readability, and overall structure.