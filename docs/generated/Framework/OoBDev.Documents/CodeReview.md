As a senior software engineer/solutions architect, I will review the code and provide suggestions for improvements.

**DocumentTypeTools.cs:**

1. The `GetByFileHeader` method could be improved by taking advantage of LINQ methods. It appears that you are trying to find the first `IDocumentType` that matches the file header. You could use `FirstOrDefault` with a lambda expression to simplify the code.
2. In the `DetectContentTypeAsync` method, you could use the null-conditional operator `?.` to avoid calling `DetectContentTypeAsync` if `_contentTypeDetector` is null.
3. The `GetByContentType` and `GetByFileExtension` methods return the first `IDocumentType` that matches the specified criterion. If you want to return a collection of matching document types, you could use `Where` and `ToList`.
4. The class has two properties `_types` and `_contentTypeDetector`. You should consider encapsulating these properties to avoid direct access and make the class more self-contained.

**OoBDev.Documents.csproj:**

1. The project file does not specify the minimum .NET Framework version. You should consider setting it to `net8.0` to ensure compatibility with the target framework.
2. The `ImplicitUsings` property is set to `false`, which means that you need to explicitly import all necessary namespaces.

**Readme.Documents.md:**

1. The documentation appears to be well-written and provides a good overview of the library. However, consider adding examples of usage and any specific configuration options.

**ServiceCollectionExtensions.cs:**

1. The `TryAddDocumentServices` method needs to be reviewed for code organization and separation of concerns. Consider breaking it down into smaller methods, each responsible for adding a specific service.
2. The method is using a lot of `TryAdd` methods, which can be error-prone. Consider using a consistent naming convention for methods that add services (e.g., `AddDocumentService`).
3. The method is adding multiple services at once. Consider using a `using` statement or a separate method that adds specific services.

**General Suggestions:**

1. Consider using dependency injection to manage the lifetime of the `IDocumentTypeTools` instance. This would make the class more testable and easier to maintain.
2. The `IDocumentType` interface has properties like `FileExtensions` and `ContentTypes`. Consider using a separate `IDocumentTypeRegistry` interface to manage these properties. This would make the `IDocumentType` interface more focused and easier to understand.
3. The `DocumentTypeTools` class has multiple methods for detecting document types. Consider creating a separate `IDocumentTypeDetector` interface that provides a way to detect document types.
4. The code has poor naming conventions. Consider following the .NET naming conventions for classes, interfaces, and methods.

Here's an example of how you could reorganize the `DocumentTypeTools` class and encapsulate its properties:

```csharp
public sealed class DocumentTypeTools : IDocumentTypeTools
{
    private readonly DocumentTypeRegistry _registry;

    public DocumentTypeTools(DocumentTypeRegistry registry)
    {
        _registry = registry;
    }

    public async Task<string?> DetectContentTypeAsync(Stream source)
    {
        if (_registry.ContentTypeDetector is not null)
        {
            return await _registry.ContentTypeDetector.DetectContentTypeAsync(source);
        }
        // ...
    }

    // ...
}
```

In this example, the `DocumentTypeTools` class has a constructor that takes a `DocumentTypeRegistry` instance, which encapsulates the `_types` and `_contentTypeDetector` properties. The `DetectContentTypeAsync` method now uses the `DocumentTypeRegistry` instance to perform the content type detection.

This is just a sample review, and the actual improvements will depend on the specific requirements and constraints of your project.