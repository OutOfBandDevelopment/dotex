As a senior software engineer and solutions architect, I will review the provided source code and suggest changes to improve maintainability.

**Project Structure**

The project structure is not clearly defined. It's not easy to identify the main projects and their relationships. I recommend creating a separate project for the OoBDev.Extensions namespace and moving the related classes and extensions into it.

**Naming Conventions**

The naming conventions used in the code are not consistent. For example, the `AddAccessor` method is camelCase, while the `ServiceCollectionExtensions` class is PascalCase. I recommend using a consistent naming convention throughout the code.

**Extension Methods**

The extension methods in the `OoBDev.Extensions` namespace are quite generic and could be extracted into separate classes or modules for better organization.

**Code Organization**

The code is not organized into logical modules or classes. I recommend grouping related classes and extensions into separate modules or classes.

**Type Usings**

The type usings at the top of the `OoBDev.Extensions` namespace are not needed. It's better to import the modules or classes that require specific types explicitly.

**Code Comments**

There are very few comments in the code. I recommend adding more comments to explain the purpose and functionality of each class, method, and extension.

**Code Duplication**

There is some code duplication in the `StringTools` class. For example, the `SplitBy` method has a loop that checks for the length of each line and adjusts it accordingly. This could be extracted into a separate method.

**Performance**

The `StringTools` class's `SplitBy` method has a loop that reads lines from a `StringReader` object. This could be optimized by using a `StreamReader` object instead, which provides better performance for large files.

**Type Inference**

The `SplitBy` method uses the `where classtype` syntax, which is not necessary. I recommend removing it to improve code readability.

**Dispose**

The `using` statement in the `SplitBy` method is not necessary. The `StringReader` object is read-only, so it's not necessary to dispose it.

**Refactoring Suggestion**

Based on the review, I recommend refactoring the code as follows:

1. Create a separate project for the OoBDev.Extensions namespace.
2. Organize the code into logical modules or classes.
3. Extract the extension methods into separate classes or modules.
4. Improve code comments to explain the purpose and functionality of each class, method, and extension.
5. Remove code duplication.
6. Optimize the `StringTools` class's `SplitBy` method by using a `StreamReader` object instead.
7. Remove unnecessary type usings.
8. Remove unnecessary `where classtype` syntax.
9. Refactor the `GetSingletonInstance` method to use a more modern and efficient approach, such as using a dictionary to store instances.

Here is an example of the refactored code:
```csharp
// OoBDev.Extensions.StringTools.cs
public static class StringTools
{
    public static IEnumerable<string> SplitBy(this string input, int length = 80, char breaker = ' ')
    {
        var reader = new StreamReader(input);
        string currentLine = string.Empty;

        while (!reader.EndOfStream)
        {
            var chunk = reader.ReadLine();

            if (chunk.Length > length)
            {
                // Split the line into chunks and yield them
                int nextStart = 0;

                while (nextStart < chunk.Length)
                {
                    var end = nextStart + length;

                    if (end > chunk.Length)
                    {
                        end = chunk.Length;
                    }

                    var chunk = chunk.Substring(nextStart, end);

                    yield return chunk.TrimEnd('\r', '\n', ' ');

                    nextStart = end;
                }
            }
            else
            {
                yield return chunk.TrimEnd('\r', '\n', ' ');
            }
        }
    }

    public static string WriteAsLines(this IEnumerable<string> lines) => string.Join(Environment.NewLine, lines);
}

// OoBDev.Extensions.ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessor<TService>(this IServiceCollection services) where TService : class
    {
        services.TryAddSingleton(typeof(IAccessor<>), typeof(Accessor<>));
        return services;
    }

    public static IServiceCollection GetSingletonInstance<TService, TInstance>(this IServiceCollection services, out TInstance instance) where TService : class where TInstance : class, TService, new()
    {
        instance = services.FirstOrDefault(i => i.ServiceType == typeof(TService))?.ImplementationInstance as TInstance ?? new TInstance();
        services.Replace(ServiceDescriptor.Singleton<TService>(instance));
        return services;
    }
}
```
Please note that this is just an example and you should adjust the code to fit your specific requirements and needs.