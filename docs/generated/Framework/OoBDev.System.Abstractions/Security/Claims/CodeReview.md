As a senior software engineer/solutions architect, I'll review the code and provide suggestions for improvements.

**ClaimPrincipalExtensions.cs**

1. The `GetAllClaims` method is a good start, but it could be improved by returning an optional list instead of `null`. This way, the method can handle both successful and failed principal cases:
```csharp
public static IEnumerable<Claim> GetAllClaims(this ClaimsPrincipal principal) =>
    principal?.Identities?.SelectMany(c => c.Claims) ?? Enumerable.Empty<Claim>();
```
2. The `GetClaimValues` method is a good candidate for using LINQ's `GroupBy` and `Select` methods to simplify the logic:
```csharp
public static IEnumerable<(string claim, string value)> GetClaimValues(this ClaimsPrincipal principal, params string[] claims) =>
    claims.Select(claim => principal.GetAllClaims().FirstOrDefault(c => c.Type == claim) ?? (claim, null)).Where(t => t.Item2 != null);
```
This will group the claims by their types and select only the claims that have a value.

3. The `GetClaimValue` method is a good candidate for using LINQ's `FirstOrDefault` method instead of `FirstOrDefault` followed by `Select`:
```csharp
public static (string claim, string value)? GetClaimValue(this ClaimsPrincipal principal, params string[] claims) =>
    claims.Select(claim => principal.GetAllClaims().FirstOrDefault(c => c.Type == claim)).FirstOrDefault(t => t != null);
```
This will find the first claim that matches one of the provided claim types and return its value.

**CommonClaims.cs**

1. The `CommonClaims` class is a good start, but it would be better to use an enum instead of public constants. This would allow for more flexibility and better code readability:
```csharp
public enum CommonClaimType
{
    UserId = "app__user_id",
    Culture = "app__user_culture",
    ExtendedProperties = "app__extended_property",
    ApplicationRight = "app__application_right",
    ObjectId = "objectid",
    ObjectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier",
    NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
}
```
Then, you can use the enum values instead of the constant strings.

**Suggested structure**

I would suggest organizing the code into separate projects and classes to improve maintainability and scalability:

* `OoBDev.System.Security.Claims` project:
	+ `ClaimsExtensions` namespace: contains the `ClaimsPrincipalExtensions` class.
	+ `CommonClaims` namespace: contains the `CommonClaimType` enum and other common claims-related types.
* `OoBDev.System.Security` project:
	+ `Claims` namespace: contains the `Claim` class and other claims-related types.

By separating the concerns into different projects and namespaces, you can better manage the dependencies and keep the code organized.

**Methods and classes**

The `ClaimsPrincipalExtensions` class has a single responsibility of extending the `ClaimsPrincipal` class. I would consider breaking it down into smaller classes or extensions that focus on specific aspects of claim management, such as:

* `ClaimExtensions`: contains methods for manipulating individual claims.
* `ClaimSetExtensions`: contains methods for working with collections of claims.
* `UserExtensions`: contains methods for working with user-related claims.

By breaking down the `ClaimsPrincipalExtensions` class, you can make the code more maintainable and scalable.

**Naming conventions**

Please follow .NET naming conventions for classes, methods, and variables:

* Class names should be Pascal-cased (e.g., `ClaimsPrincipalExtensions`, not `claimsPrincipalExtensions`).
* Method names should be Pascal-cased (e.g., `GetClaimValues`, not `getClaimValues`).
* Variable names should be camel-cased (e.g., `claims`, not `Claims`).

Overall, the code is well-structured, and the suggestions above are meant to improve its maintainability, scalability, and readability.