I'll review the code as a senior software engineer/solutions architect and suggest changes to improve maintainability, scalability, and adherence to best practices.

**AdditionalSwaggerGenEndpointsOptions**
1. Remove the `using` statements from the class definition. They're not necessary and make the class harder to read. Instead, use the language to import the necessary namespaces.
2. Instead of using a constructor with multiple parameters, consider using a constructor with fewer parameters (e.g., only `ILogger<AdditionalSwaggerGenEndpointsOptions>`) and injecting the other dependencies through properties. This makes the class easier to test and maintain.
3. Remove the `IConfigureOptions<SwaggerGenOptions>` interface and instead use a generic method (`Configure<TOptions>`) that takes an instance of the desired options class (e.g., `SwaggerGenOptions`).
4. Simplify the `ResolveSchemaType` method using a switch statement or a dictionary lookup.
5. Consider moving the `fileVersions` collection to a separate method, as it's a complex calculation that could be isolated from the main configuration code.

**AdditionalSwaggerUIEndpointsOptions**
1. Similarly, remove the `using` statements from the class definition.
2. Consider injecting the `IActionDescriptorCollectionProvider` instance through a property instead of a constructor parameter.
3. Simplify the `SwaggerEndpoint` configuration using a foreach loop or LINQ instead of a complex expression.
4. Consider adding a check for null or empty groups to avoid errors.

**AddMvcFilterOptions**
1. Simplify the constructor by removing the type parameter `<TFilter>` and injecting it as a dependency through the `AddFilter` method.
2. Consider adding a check for the filter's `IsValidForRequest` method to ensure it's only added for applicable requests.

**AddOperationFilterOptions** and **AddSchemaFilterOptions**
1. These classes are very similar; consider merging them into a single class (`AddFilterOptions`) with a type parameter `<T>` that can be either an `IOperationFilter` or an `ISchemaFilter`.
2. Simplify the constructor by removing the type parameter `<T>` and injecting the filter instance as a dependency through the `AddFilter` method.

**General suggestions**

1. Consider using a more consistent naming convention throughout the codebase (e.g., PascalCase for classes and variables, camelCase for methods and properties).
2. Use meaningful names for variables and parameters, and avoid using single-letter variable names.
3. Consider separating the concerns of the `AdditionalSwaggerGenEndpointsOptions` class by breaking it into smaller, more focused classes.
4. Keep the code organized by separating the responsibilities of each class and using clear, concise method and property names.
5. Use logging only for errors and exceptions, and ensure that log messages are meaningful and helpful for debugging.

By implementing these suggestions, you can improve the maintainability, scalability, and readability of the codebase.