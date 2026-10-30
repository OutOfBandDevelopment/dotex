# Example AppHost live journeys: open problems

**Status:** Open, 2026-10-29.

- The Keycloak sign-in and token-claims journeys fail against the host, because it runs the API anonymously (`IDENTITY_PROVIDER=None`). Either wire Keycloak into the host or skip those journeys when the API is anonymous.
- The host passes the dashboard URL, API key, service name and source to the API, and the "Open the dashboard" link works. The dashboard lists no resources (`/api/telemetry/resources` is empty), so the API traces do not reach it and "See what the API just did" still fails live. Find why the OTLP export from the host-run API is not arriving.
- The dacpac build crashed once with exit code -1073741819 and passed on retry.
