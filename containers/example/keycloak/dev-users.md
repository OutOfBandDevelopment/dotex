# Development users

[Integration test infrastructure](../../testing/README.md)

The `oobdev-example` Keycloak realm is imported from [oobdev-example-realm.json](oobdev-example-realm.json) when the example or integration-test stack starts. The accounts are fixed (same ids, names and passwords on every machine) so journeys, screenshots and manual testing behave the same everywhere.

**These accounts are for local development only.** Their passwords equal their user names. Never import this realm into a shared or production Keycloak.

## Contents

- [Sample users](#sample-users)
- [Roles](#roles)
- [Client](#client)
- [Other accounts in the stack](#other-accounts-in-the-stack)
- [Changing the realm](#changing-the-realm)

## Sample users

**Table 1 — Sample users in the oobdev-example realm**

| User | Password | Name | Roles |
| --- | --- | --- | --- |
| `ada` | `ada` | Ada Lovelace | reader |
| `alan` | `alan` | Alan Turing | reader, editor |
| `margaret` | `margaret` | Margaret Hamilton | reader, operator |
| `grace` | `grace` | Grace Hopper | reader, editor, operator, admin |

Each user has the email `<user>@example.test`, a verified email, and a fixed id `00000000-0000-4000-8000-00000000000N` (N = 1 for ada through 4 for grace). The Playwright journeys sign in as `ada` unless `E2E_USER` and `E2E_PASSWORD` are set.

## Roles

**Table 2 — Realm roles**

| Role | Meaning |
| --- | --- |
| `reader` | Can read and try the example screens |
| `editor` | Can change content: templates, documents, messages |
| `operator` | Can run operations screens: queues, cache, health |
| `admin` | Can use every example screen, including identity management |

Roles arrive in the access token's `realm_access.roles` claim; the API translates roles into application rights in middleware (see the authorization practices in the patterns discovery documents).

## Client

**Table 3 — Clients**

| Client id | Type | Use |
| --- | --- | --- |
| `oobdev-example-web` | public, authorization code with PKCE | The Vue single page application. Redirect URIs: `http://localhost:8088/*`, `http://localhost:5173/*`, `http://localhost:8080/*`. Its tokens carry the audience `oobdev-example-api`. |

## Other accounts in the stack

**Table 4 — Other accounts**

| Where | User | Password | Use |
| --- | --- | --- | --- |
| Keycloak admin console (`http://localhost:${TEST_PORT_KEYCLOAK:-8081}`) | `admin` | `admin` | Administer realms |
| `integration-test` realm | `testuser` / `adminuser` | `testpassword` / `adminpassword` | Integration tests |

## Changing the realm

Keycloak imports a realm only when it does not exist yet. After editing the JSON, remove the Keycloak data volume so it is imported again:

```
docker compose -f containers/example/docker-compose.example.yml rm -sf keycloak
docker volume rm oobd-test-keycloak-data
docker compose -f containers/example/docker-compose.example.yml up -d keycloak
```
