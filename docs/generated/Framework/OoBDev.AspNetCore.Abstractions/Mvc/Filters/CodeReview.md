As a senior software engineer/solutions architect, I'll provide a review of the provided source code and suggest changes to improve maintainability, modularity, and scalability.

**ApplicationRightAttribute.cs:**

1. Consistent naming convention: Rename `Rights` to `RequiredRights` for better readability and consistency.
2. Use a more descriptive naming convention for the constructor parameter. Instead of `rights`, use `requiredApplicationRights`.
3. Consider adding a check for null or empty `requiredApplicationRights` in the constructor to ensure the attribute is properly initialized.

Updated code:
```csharp
public class ApplicationRightAttribute : TypeFilterAttribute<ApplicationRightRequirementFilter>
{
    public string[] RequiredRights { get; }

    public ApplicationRightAttribute(params string[] requiredApplicationRights)
    {
        RequiredRights = requiredApplicationRights ?? new string[0];
        Arguments = new[] { requiredApplicationRights };
    }
}
```

**ApplicationRightRequirementFilter.cs:**

1. Consistent naming convention: Rename `rights` to `requiredRights` for better readability and consistency.
2. Use early returns instead of nested if-else statements to simplify the `OnAuthorization` method.
3. Consider extracting the `ForbidResult` creation into a separate method to reduce code duplication.
4. Instead of `Any` method, use LINQ's `Contains` method to simplify the condition.
5. Consider making `Any` method an extension method for `IEnumerable<string>` to make the code more readable.

Updated code:
```csharp
public class ApplicationRightRequirementFilter : IAuthorizationFilter
{
    private readonly IReadOnlyList<string> _requiredRights;

    public ApplicationRightRequirementFilter(string[] requiredRights)
    {
        _requiredRights = requiredRights;
    }

    public virtual void OnAuthorization(AuthorizationFilterContext context)
    {
        var userAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated;
        var userRights = context.HttpContext.User.GetClaimValues(CommonClaims.ApplicationRight);

        if (!userAuthenticated)
            Forbid(context);

        if (_requiredRights.Any() && !_requiredRights.Any(userRights.Select(c => c.value)))
            Forbid(context);
    }

    internal void Forbid(AuthorizationFilterContext context)
    {
        context.Result = new ForbidResult();
    }
}

public static class Extensions
{
    public static bool Contains(this IEnumerable<string> items, string value) => items.Any(i => i == value);
}
```

**Suggestions for improvement:**

1. Consider extracting the `GetClaimValues` method into a separate class or extension method for better reusability.
2. Review the naming conventions for classes, methods, and variables to ensure they are consistent throughout the codebase.
3. Think about creating a separate interface for the `ApplicationRightRequirementFilter` to make it more decoupled and easier to test.
4. Consider implementing logging or auditing for the `Forbid` scenario to provide additional insight into authorization failures.

By implementing these suggestions, the code will become more maintainable, scalable, and easier to understand and extend.