# Example AppHost live journeys: open problems

**Status:** Open, 2026-10-29.

- The Keycloak sign-in and token-claims journeys fail against the host, because it runs the API anonymously (`IDENTITY_PROVIDER=None`). Either wire Keycloak into the host or skip those journeys when the API is anonymous.
- The host passes the dashboard URL, API key, service name and source to the API, and the "Open the dashboard" link works. The dashboard lists no resources (`/api/telemetry/resources` is empty), so the API traces do not reach it and "See what the API just did" still fails live. Find why the OTLP export from the host-run API is not arriving.
- The dacpac build crashed once with exit code -1073741819 and passed on retry.

**Decision (owner, 2026-11-04):** the Example AppHost runs Keycloak as a container under Aspire (realm import as in the compose stack), starts the API with `IDENTITY_PROVIDER=Keycloak` and the authority from the Keycloak endpoint, and gives the web client that address. The telemetry journey is diagnosed on a live host run afterwards.

**Built (2026-11-04):** `Program.cs` mounts the example realm beside the integration realm, publishes Keycloak on a fixed host port (`TEST_PORT_KEYCLOAK`, default 8081, so the issuer `http://localhost:<port>/realms/oobdev-example` is stable), starts the API with `IDENTITY_PROVIDER=Keycloak` (authority, audience, issuer and OpenAPI URLs as in the compose file), waits for the container, and gives the Vite app `VITE_OIDC_AUTHORITY` and `VITE_OIDC_CLIENT_ID`. The project builds; the live run and the telemetry diagnosis are still to do.
