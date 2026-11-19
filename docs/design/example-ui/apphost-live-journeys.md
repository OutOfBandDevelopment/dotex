# Example AppHost live journeys: open problems

**Status:** Open, 2026-10-29.

- The Keycloak sign-in and token-claims journeys fail against the host, because it runs the API anonymously (`IDENTITY_PROVIDER=None`). Either wire Keycloak into the host or skip those journeys when the API is anonymous.
- The host passes the dashboard URL, API key, service name and source to the API, and the "Open the dashboard" link works. The dashboard lists no resources (`/api/telemetry/resources` is empty), so the API traces do not reach it and "See what the API just did" still fails live. Cause found 2026-11-05: the exporter forced HttpProtobuf over Aspire's gRPC endpoint; fixed in `OoBDev.Telemetry` (live confirmation pending).
- The dacpac build crashed once with exit code -1073741819 and passed on retry.

**Decision (owner, 2026-11-04):** the Example AppHost runs Keycloak as a container under Aspire (realm import as in the compose stack), starts the API with `IDENTITY_PROVIDER=Keycloak` and the authority from the Keycloak endpoint, and gives the web client that address. The telemetry journey is diagnosed on a live host run afterwards.

**Built (2026-11-04):** `Program.cs` mounts the example realm beside the integration realm, publishes Keycloak on a fixed host port (`TEST_PORT_KEYCLOAK`, default 8081, so the issuer `http://localhost:<port>/realms/oobdev-example` is stable), starts the API with `IDENTITY_PROVIDER=Keycloak` (authority, audience, issuer and OpenAPI URLs as in the compose file), waits for the container, and gives the Vite app `VITE_OIDC_AUTHORITY` and `VITE_OIDC_CLIENT_ID`. The project builds; the live run and the telemetry diagnosis are still to do.

**Live run (2026-11-19):** the host started with Keycloak on `TEST_PORT_KEYCLOAK=8082` (the test stack holds 8081); the example realm answered its OpenID configuration. The dashboard's telemetry API was off, so the launch profile now enables it with a fixed key (`DASHBOARD__API__*`, and `AppHost__DashboardApiKey` for the API). After requests to the API, `/api/telemetry/resources` lists `example-api` with logs and traces, so the OTLP fix works live. Still not run: the Playwright journeys against this host.
