As a senior software engineer/solutions architect, I'll provide feedback on the code structure, patterns, and maintainability. Here are some suggestions for improvement:

**ConfigureOAuthSwaggerGenOptions.cs**

* The class is doing too much. It's responsible for configuring SwaggerGen options, getting OAuth2 scopes, and logging warnings. Consider splitting it into two classes: one for configuring SwaggerGen options and another for getting OAuth2 scopes.
* The `GetScopes` method could be moved to a separate class or an extension method to make the code more reusable.
* Consider using a more robust logging mechanism, such as Serilog, instead of `Microsoft.Extensions.Logging`.
* The `Configure` method is doing too much processing. It's better to break it down into smaller, more focused methods.

**ConfigureOAuthSwaggerUIOptions.cs**

* This class is also doing too much. It's responsible for configuring SwaggerUI options and logging warnings. Consider splitting it into two classes: one for configuring SwaggerUI options and another for logging warnings.
* The `Configure` method is doing too much processing. It's better to break it down into smaller, more focused methods.
* Consider using a more robust logging mechanism, such as Serilog, instead of `Microsoft.Extensions.Logging`.

**OAuth2SwaggerOptions.cs**

* The class is quite simple and could be replaced with a dedicated configuration section in the appsettings.json file.
* Consider using a more robust configuration mechanism, such as Serilog, instead of `IOptions`.

**Code Structure**

* The code is quite tightly coupled to specific frameworks and libraries. Consider using more abstract interfaces and abstractions to decouple the code from specific implementations.
* The code is using raw HTTP URLs for the authorization and token endpoints. Consider using a more robust mechanism, such as Nancy or a dedicated OAuth library, to handle these endpoints.

**Naming Conventions**

* Some of the method and variable names are not following the conventional naming conventions. For example, methods like `GetScopes` or `Configure` are not descriptive enough.
* Consider using more descriptive and precise names for methods and variables, following the conventional naming conventions.

**Code Review**

Here's a high-level review of the code:

* The code is generally well-structured and follows the conventional coding standards.
* However, the classes are doing too much, and the code could be refactored to be more modular and reusable.
* The logging mechanism is not robust enough, and it's better to use a more advanced logging framework.
* The code could benefit from more descriptive and precise names for methods, variables, and parameters.

**Refactoring Suggestions**

1. Extract the `GetScopes` method to a separate class or extension method.
2. Break down the `Configure` method in `ConfigureOAuthSwaggerGenOptions.cs` into smaller, more focused methods.
3. Consider using a more robust configuration mechanism, such as Serilog, instead of `IOptions`.
4. Extract the OAuth2 scope processing to a separate class or extension method.
5. Consider using a more advanced logging framework, such as Serilog.
6. Improve the naming conventions for methods, variables, and parameters.
7. Refactor the `ConfigureOAuthSwaggerUIOptions.cs` class to be more modular and reusable.
8. Consider using a more robust mechanism, such as Nancy or a dedicated OAuth library, to handle the authorization and token endpoints.

By addressing these concerns and suggestions, the code can become more maintainable, modular, and robust.