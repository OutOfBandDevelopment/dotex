**Code Review**

Overall, the code appears to be well-structured, and the classes follow standard .NET naming conventions and best practices. However, there are some areas that can be improved to make the code more maintainable and scalable.

**Suggested Changes**

1. **Separate concerns**: The CompletionRequest and CompletionResponse classes have both required and optional properties. Consider separating the optional properties into a separate class or interface, allowing for a cleaner and more consistent API.
2. **Immutability**: The CompletionRequest and CompletionResponse classes use the `init` accessor to ensure immutability. This is a good practice, but it's essential to ensure that the classes are truly immutable by making all non-`init` properties read-only.
3. **Naming conventions**: The KeyValuePairModel class has camelCase property names (e.g., `Key` and `Value`), while the CompletionRequest and CompletionResponse classes use PascalCase (e.g., `Model` and `Context`). To maintain consistency, consider using a single naming convention throughout the codebase.
4. **Type safety**: The `long[]?` and `string[]?` properties in the CompletionRequest class can be replaced with a strongly-typed collection, such as `ImmutableArray<long>` or `List<string>`, to ensure type safety.

**Refactored Code**

Here are the refactored classes:
```csharp
/// <summary>
/// Represents a completion request with optional properties.
/// </summary>
public record CompletionRequestCore
{
    public required string Prompt { get; init; }

    public string? Model { get; init; }
    public List<string>? Images { get; init; }
    public string? System { get; init; }
    public string? Template { get; init; }
}

/// <summary>
/// Represents a completion request with required context and response.
/// </summary>
public record CompletionRequest(CompletionRequestCore core, long[] context, string response);

/// <summary>
/// Represents a key-value pair model.
/// </summary>
public class KeyValuePairModel
{
    public required string Key { get; }
    public required string Value { get; }
}
```
**Additional Suggestions**

1. **Consider using interfaces**: Instead of concrete classes, consider defining interfaces for the KeyValuePairModel and CompletionRequestCore classes. This would allow for more flexibility and extensibility in the codebase.
2. **Use a logging mechanism**: Add a logging mechanism to track any exceptions or unexpected behavior in the code. This can help with debugging and troubleshooting.
3. **Code organization**: Consider reorganizing the code into separate folders or projects based on functionality, such as `Models`, `Services`, `Repositories`, etc.
4. **Documentation**: Add comments and documentation to the code to provide context and explain the purpose of each class, method, and property.

Overall, the code appears to be well-organized and follows standard .NET best practices. With these suggested changes, the code will become even more maintainable and scalable.