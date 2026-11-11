# OoBDev.Keycloak

Read-only Keycloak adapter for the identity abstractions. It reads a realm through the admin REST API with a service account (client credentials); nothing creates, changes or removes users (owner, 2026-11-10).

## Settings (`KeycloakIdentityOptions`)

| Key | Meaning |
|-----|---------|
| `BaseUrl` | Server root, for example `http://localhost:8081` |
| `Realm` | Realm to read |
| `ClientId`, `ClientSecret` | Service account client; it needs the `view-users` and `view-realm` realm-management roles |

**Table 1 — settings**

## Types

- `KeycloakDirectoryReader`: `ListUsersAsync(search, email, max)` returns `KeycloakUserSummary` (id, user name, email, names, enabled, direct realm roles); `ListRolesAsync()` returns the realm role names. The token is cached until 30 seconds before it expires.
- `ManageKeycloakUser` (`IIdentityManager`): `GetIdentityUsersByEmail` matches the email exactly; `CreateIdentityUserAsync` and `RemoveIdentityUserAsync` throw `NotSupportedException`.
- `TryAddKeycloakServices(configuration, section)` binds the options, registers the reader as a typed `HttpClient` and the manager.

Unit tests use a scripted handler; the Integration tests in `OoBDev.Keycloak.Tests` need the Keycloak container.
