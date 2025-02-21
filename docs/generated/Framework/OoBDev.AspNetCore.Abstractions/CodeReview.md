As a senior software engineer/solutions architect, I'll provide a comprehensive review of the provided source code and suggest changes to make it more maintainable.

**Project File (OoBDev.AspNetCore.Abstractions.csproj)**

Overall, the project file looks well-organized. However, a few suggestions can improve maintainability:

1. **Consistent indentation**: There's inconsistent indentation across the file. Use a consistent 4-space indentation throughout.
2. **Remove unnecessary spaces**: Remove extra spaces between elements, such as between `<FrameworkReference>` and `<ItemGroup>`.
3. **Group similar settings**: Group related settings together, such as `<PropertyGroup>` and `<ItemGroup>` sections.

**Readme File (Readme.AspNetCore.Abstractions.md)**

The README file provides a good summary of the library. However, consider the following improvements:

1. **Use a concise outline**: Update the summary section to provide a concise outline of the library's main components and features.
2. **Add more details**: Add more details about the `ApplicationRightAttribute` and `ApplicationRightRequirementFilter` classes, such as their intended use cases, examples, or known limitations.
3. **Consider using an markdown guide**: Follow a markdown guide (e.g., GitHub Flavored Markdown) to ensure the README file is properly formatted.

**Code Structure and Class Suggestions**

Based on the provided information, it's hard to determine the actual code structure and class design. However, a few suggestions can improve maintainability:

1. **Separate concerns**: Consider separating concerns by creating separate projects for different aspects of the library (e.g., one for filters, another for authentication).
2. **Use interfaces and abstractions**: Define interfaces and abstractions for the `ApplicationRightAttribute` and `ApplicationRightRequirementFilter` classes to promote flexibility and extensibility.
3. **Extract utility classes**: Extract utility classes for processing rights and authentication, making the code more modular and reusable.
4. **Consider using dependency injection**: Use dependency injection to supply dependencies to the `ApplicationRightRequirementFilter` class, allowing for easier testing and reusability.

**Additional Suggestions**

1. **Code comments**: Add code comments to explain complex logic, variable names, and function requirements.
2. **Error handling**: Implement robust error handling mechanisms to handle unexpected scenarios and edge cases.
3. **Testing**: Develop comprehensive unit tests and integration tests to ensure the library's correctness and reliability.

In summary, the project file and README file are well-structured, but with a few tweaks, they can become even more maintainable. The code structure and class design require more information, but the suggestions provided can improve maintainability and extensibility.