I'll provide a review of the source code as a senior software engineer/solutions architect, suggesting changes to coding patterns, methods, structure, and classes to make the code more maintainable.

**ReflectionExtensions.cs**

* The class has many static methods, which might be a good candidate for a utility class.
* The use of `BindingFlags` constants is good, but consider using a more descriptive name for these constants instead of just `PublicProperties` or `PublicStaticMethod`.
* The `MakeSafe` method is complex and has many permutations. Consider breaking it down into smaller, more focused methods.
* The `TryParse` method has many conditions and exceptions. Consider simplifying it using a more straightforward approach.

Proposed changes:
* Extract the `MakeSafe` method into smaller methods for specific parsing schemes.
* Simplify the `TryParse` method by reducing the number of conditions and exceptions.

**ResourceExtensions.cs**

* The class has many static methods that seem to work with different types of resources (context, type, assembly). Consider creating separate classes or interfaces for each type of resource.
* The method names are unclear, e.g., `GetResourceStream` and `GetResourceAsStringAsync`. Consider using more descriptive names that indicate the purpose of the method.
* The use of `null` checks in the `GetResourceStream` methods can be simplified using the null-conditional operator (`?.`) or early returns.

Proposed changes:
* Extract the resource-related logic into separate classes or interfaces for each type of resource (e.g., `ResourceManager` for context-related resources, `AssemblyResourceManager` for assembly-related resources).
* Simplify the null checks using the null-conditional operator or early returns.

**Class Structure**

The code has many static methods in the `ReflectionExtensions` class and the `ResourceExtensions` class. While these methods might be useful, there are concerns about:

* Coupling between classes: The methods in these classes seem to operate on different types of objects (context, type, assembly).
* Modularity: The classes seem to have responsibilities that are not clearly defined.

Proposed changes:

* Extract the resource-related logic into separate classes or interfaces for each type of resource (e.g., `ResourceManager` for context-related resources, `AssemblyResourceManager` for assembly-related resources).
* Consider creating an interface or abstract class for `ReflectionExtensions` that defines the common behavior for reflection-related operations.

**Code Style and Consistency**

The code has some inconsistent naming conventions (e.g., `MakeSafe` vs. `TryParse`). Consider adopting a consistent naming convention throughout the code.

Proposed changes:

* Adopt a consistent naming convention throughout the code (e.g., PascalCase for methods and properties, camelCase for local variables).

Overall, the code seems to be a good starting point, but could benefit from some refactoring to improve maintainability, modularity, and consistency. By extracting the resource-related logic into separate classes, simplifying the methods, and adopting a consistent naming convention, the code can be made more maintainable and easier to use.