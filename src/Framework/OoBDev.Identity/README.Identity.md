# OoBDev.Identity

Default `IUserManagementProvider`: creates a user account by delegating to whichever identity manager the application registers. Contracts are in [OoBDev.Identity.Abstractions](../OoBDev.Identity.Abstractions/README.Identity.Abstractions.md); the implementation for Keycloak is `OoBDev.Keycloak` (`ManageKeycloakUser`). Applications validate tokens and never store passwords (rule `owner-decisions`), so this provider only provisions accounts in the identity provider.

## Use

```csharp
services.TryAddIdentityServices(configuration);   // IUserManagementProvider
services.TryAddKeycloakServices(...);             // an IIdentityManager (Keycloak adapter)

var created = await provider.CreateAccountAsync(new UserCreateModel
{
    FirstName = "Ada", LastName = "Lovelace", EmailAddress = "ada@example.org",
});
// created.Username is the identity provider's user id, created.Password the generated password (empty when none)
```

`UserManagementProvider` (transient, virtual so applications can extend it) calls `IIdentityManager.CreateIdentityUserAsync(email, first, last)` and maps the returned id and password to a `UserCreatedModel`. Without a registered `IIdentityManager` it throws `NotSupportedException` ("No identity management provider registered"). The `configuration` argument of `TryAddIdentityServices` is currently unused.

## Tests

`OoBDev.Identity.Tests` has Unit tests for the mapping, the missing-manager failure and the registration.
