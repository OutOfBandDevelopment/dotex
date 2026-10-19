# Running the example

[Example documentation](README.md)

The example runs as one Docker Compose project, `oobd-example`, defined in `containers/example/docker-compose.example.yml`. It includes the integration test stack (`containers/testing`) so each backend service is defined once.

## Contents

- [Commands](#commands)
- [Services and ports](#services-and-ports)
- [Signing in](#signing-in)
- [Optional settings](#optional-settings)
- [Aspire host](#aspire-host)

## Commands

**Table 1 — Scripts**

| Command | Effect |
| --- | --- |
| `scripts/examples/example-up.ps1` | Builds and starts the stack, waits until the SPA answers, opens the browser |
| `scripts/examples/example-down.ps1` | Stops the stack |
| `scripts/examples/example-down.ps1 -Clean` | Stops it and removes volumes and locally built images |

The first start downloads images and the Ollama models, which takes a while.

## Services and ports

**Table 2 — What runs and where**

| Service | Host port (default) | Override | Used by |
| --- | --- | --- | --- |
| `example-web` (nginx, SPA) | 8088 | `EXAMPLE_PORT_WEB` | You; proxies `/api`, `/Document`, `/scalar`, `/asyncapi`, `/openapi` to the API |
| `example-api` | none (inside the network, 8080) | | The SPA |
| `keycloak` | 8081 (the script uses 18081) | `TEST_PORT_KEYCLOAK` | Sign-in |
| `ollama` | 11435 (the script uses 21435) | `TEST_PORT_OLLAMA` | Chat and embeddings |
| `smtp4dev` | web 7777, SMTP 25, IMAP 143 | `TEST_PORT_SMTP_WEB`, `TEST_PORT_SMTP`, `TEST_PORT_IMAP` | Communications |
| `rabbitmq` | AMQP 5673, management 15672 | `TEST_PORT_RABBITMQ`, `TEST_PORT_RABBITMQ_MGMT` | Queues |
| `plantuml` | 18080 | `TEST_PORT_PLANTUML` | Documentation validation |

Every host port of the test stack can be overridden with `TEST_PORT_<NAME>`; see [the integration test infrastructure](../architecture/testing/integration/README.md). The scripts pick the Keycloak and Ollama ports above so the stack starts next to a locally installed Keycloak or Ollama.

## Signing in

Sign-in is on by default. Use the sample users in [dev-users.md](../../containers/example/keycloak/dev-users.md) (for example `ada` / `ada`).

## Optional settings

Copy secrets and overrides into `containers/example/.env` (git-ignored) using the .NET configuration form, for example `GroqCloudApiClientOptions__ApiKey=...`. Set `EXAMPLE_IDENTITY_PROVIDER=None` and an empty `EXAMPLE_OIDC_AUTHORITY=` to run without sign-in.

## Aspire host

`src/Examples/OoBDev.Example.AppHost` runs the same example under .NET Aspire for development with a debugger. See its readme.
