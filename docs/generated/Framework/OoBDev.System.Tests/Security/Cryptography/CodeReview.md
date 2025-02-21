As a senior software engineer/solutions architect, I'll provide feedback on the code structure, maintainability, and suggest improvements.

**HashSelectorTest.cs**

* The use of `required` keyword for properties is a good practice, but it's worth noting that it was introduced in C# 9.0. If you're still using an earlier version, you can simply declare the property with the `get` and `set` keywords.
* The `TestContext` property should be replaced with a more descriptive name, such as `TestHelper`.
* The `DataTestMethod` attribute is used to run the test with different data sets. This is a good practice, but you might consider using a more descriptive name for the attribute, such as `TestWithDifferentInput`.
* The `TryAddSystemExtensions` method seems unused and can be removed.
* The `GetRequiredKeyedService` method is used to resolve a service instance. This can be better implemented using Microsoft.Extensions.DependencyInjection's `ServiceDescriptor` to register services.

**Md5HashTests.cs**, **Sha256HashTests.cs**, and **Sha512HashTests.cs**

* Each test class has a similar structure and tests with the same pattern (input, expected output). You can consider merging these classes into a single class, with different test methods for each hash type.
* The test methods are using `DataTestMethod` attribute, but the test data is hardcoded. You can improve the test data management by using a separate data provider class or a CSV file.
* The test methods are printing the input and output to the console. While this can be helpful during development, it's recommended to remove it in the production-ready code.
* The `GetHash` method is being tested with a specific input and expected output. You can improve the unit test by using more different inputs to test the hash function.
* Consider using a test factory to create the hash instance instead of creating it manually.

**Improvement Suggestions**

1. **Decouple dependencies**: Instead of using `IConfiguration` and `IHash` directly in the test classes, consider using dependency injection to inject these dependencies.
2. **Use interfaces**: Use interfaces to define the `IHash` and other dependencies, which will make it easier to change the implementation if needed.
3. **Factor out common functionality**: Consider creating a base class for the hash test classes to share common functionality.
4. **Improve test data management**: Use a separate data provider class or a CSV file to manage the test data instead of hardcoding it.
5. **Use a test framework**: Consider using a popular test framework like NUnit or xUnit, which offers more features and flexibility than the built-in Microsoft.VisualStudio.TestTools.UnitTesting.
6. **Avoid coupling test methods**: Each test method should be independent and not rely on the outcome of another test method.
7. **Keep the test classes separate**: While you can consider merging the test classes for simplicity, it's recommended to keep each test class separate to maintain a clear separation of concerns.

**Code Structure**

The code structure appears to be following the Test-Driven Development (TDD) style, which is a good practice. The test classes are separate from the main code, and the tests are focused on specific functionality. To improve the structure, consider implementing the Single Responsibility Principle (SRP), where each test class or method has a single responsibility.

Overall, the code is well-organized, but there are some areas that can be improved for better maintainability and scalability.