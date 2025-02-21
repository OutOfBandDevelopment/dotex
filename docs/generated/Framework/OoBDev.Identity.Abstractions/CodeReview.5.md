Congratulations on having a solid start with your models! As a senior software engineer, I'd like to provide feedback and suggestions to improve maintainability, scalability, and overall design.

** Observations and Feedback **

1. **Similarities between models**: Both `UserCreateModel` and `UserIdentityModel` have similar properties (FirstName, LastName, EmailAddress). This repetition can be reduced by creating a base class or interface that these models can inherit from or implement.
2. **Accessors vs. Properties**: Your code uses both accessor methods and auto-implemented properties. Consider using a consistent approach throughout the codebase. Auto-implemented properties are often preferred as they provide a simpler and more concise way of defining properties.
3. **Validation**: Some required properties (e.g., FirstName, LastName, EmailAddress) in `UserCreateModel` might need additional validation. Consider using built-in .NET validation attributes or implementing custom validation logic.
4. **Consistent naming conventions**: Your code uses both PascalCase and camelCase (e.g., `EmailAddress` vs. `emailAddress`). Ensure that you follow a consistent naming convention throughout the codebase.

**Recommendations**

1. **Introduce a base class or interface**: Create a base class or interface that defines the common properties (FirstName, LastName, EmailAddress) and have both `UserCreateModel` and `UserIdentityModel` inherit from or implement it. This will reduce duplication and make it easier to maintain changes.
2. **Use a consistent approach for properties**: Replace accessor methods with auto-implemented properties or vice versa. Consistency is key to maintainable code.
3. **Consider using inheritance for validation**: If `UserCreateModel` requires additional validation, consider using inheritance and creating a base class that implements the validation logic.
4. **Standardize naming conventions**: Ensure that you use a consistent naming convention throughout the codebase. In C#, it's common to use PascalCase for property names.

**Code Changes**

Here's an updated version of your code incorporating these recommendations:
```csharp
// Base class or interface
public abstract class UserBase
{
    public abstract string? FirstName { get; set; }
    public abstract string? LastName { get; set; }
    public abstract string? EmailAddress { get; set; }
}

// UserCreateModel
public record UserCreateModel : UserBase
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}

// UserIdentityModel
public record UserIdentityModel : UserBase
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool? ForcePasswordChangeNextSignIn { get; set; }
}
```
By following these recommendations, you'll create a more maintainable, scalable, and consistent codebase. This will make it easier to extend and evolve your application as needed.