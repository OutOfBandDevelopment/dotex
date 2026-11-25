# OoBDev.Identity.Abstractions

Contracts and models (namespace `OoBDev.Identity`) for creating and removing user accounts in an identity provider. Implementations live in [OoBDev.Identity](../OoBDev.Identity/README.Identity.md) and the identity provider adapters.

**Table 1 — Contracts**

| Type | Members |
|------|---------|
| `IIdentityManager` | `GetIdentityUsersByEmail(email)` returns `IList<UserIdentityModel>?`; `CreateIdentityUserAsync(email, firstName, lastName)` returns the object id and an optional generated password; `RemoveIdentityUserAsync(userId)` returns whether the user was removed |
| `IUserManagementProvider` | `CreateAccountAsync(UserCreateModel)` returns a `UserCreatedModel` |

**Table 2 — Models (records)**

| Type | Properties |
|------|------------|
| `UserCreateModel` | `FirstName`, `LastName`, `EmailAddress` (all required) |
| `UserCreatedModel` | `Username`, `Password` (both required) |
| `UserIdentityModel` | Optional `FirstName`, `LastName`, `EmailAddress`, `UserName`, `Password`, `ForcePasswordChangeNextSignIn` |

Applications validate tokens and never store passwords (owner decision); the password fields carry a generated initial password back to the caller only.
