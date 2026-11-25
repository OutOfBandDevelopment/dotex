# OoBDev.Keycloak.Tests

Tests for `OoBDev.Keycloak`: Unit tests of the directory reader with a scripted HTTP handler, and Integration tests of Keycloak user management against the Keycloak container of the test stack.

## Contents

- [Prerequisites](#prerequisites)
- [Tests](#tests)
- [Configuration](#configuration)
- [Cleanup](#cleanup)
- [See also](#see-also)

## Prerequisites

The Integration tests need the stack with the `integration-test` realm:

```bash
cd containers/testing && ./scripts/integration-up.sh --wait
```

The host port defaults to 8081 (`TEST_PORT_KEYCLOAK` changes it, then pass the matching `KEYCLOAK_URL` as a test run parameter). The tests refuse to run when `KEYCLOAK_URL` is not a test instance (`TestInstanceGuard`).

## Tests

**Table 1 — Test classes**

| Class | Tier | Covers |
|-------|------|--------|
| `KeycloakDirectoryReaderTests` | Unit | Search and exact e-mail listing with sorted roles, token reuse until expiry, role listing, no match returns null. |
| `KeycloakUserManagementTests` | Integration | With `Keycloak.ApiClient.Net` and an admin token from the master realm: create a user (with credentials, with realm roles), set a password and authenticate, update, page and search users, a disabled user cannot authenticate, count users. |
| `ExampleRealmTests` | Unit and Integration | Unit: the `oobdev-example` realm file defines ada, alan, margaret and grace with the documented roles, and the example API maps the `admin` role grace holds to every right its controllers guard. Integration: the running Keycloak holds the same users and realm roles; a failure means the volume predates the realm file, so reset it (`integration-down.sh --clean`, then `integration-up.sh --wait`). |

## Configuration

Read with `TestContext.GetRequiredProperty<T>()` from `src/.runsettings`:

**Table 2 — Run parameters**

| Property | `.runsettings` value | Use |
|----------|----------------------|-----|
| `KEYCLOAK_URL` | `http://localhost:8081` | Base URL. |
| `KEYCLOAK_ADMIN_USERNAME`, `KEYCLOAK_ADMIN_PASSWORD` | `admin`, `admin` | Master realm admin used to get the admin token. |
| `KEYCLOAK_REALM` | `integration-test` | Realm the users are created in. |
| `KEYCLOAK_CLIENT_ID`, `KEYCLOAK_CLIENT_SECRET` | `integration-test-client`, test secret | Client for the authentication checks. |
| `KEYCLOAK_TEST_USERNAME`, `KEYCLOAK_TEST_PASSWORD` | `testuser`, `testpassword` | Pre-configured realm user. |

## Cleanup

Each test records the user it created and deletes it in `[TestCleanup]`; a failure to delete is written to the test output and does not fail the test. If users are left behind, list them with the admin console of the container.

## See also

- [Keycloak testing guide](../../../../containers/testing/KEYCLOAK-TESTING.md)
- [Keycloak realm configuration](../../../../containers/testing/keycloak-config/README.md)
- [Test variables](../../../../TEST_VARIABLES.md)
