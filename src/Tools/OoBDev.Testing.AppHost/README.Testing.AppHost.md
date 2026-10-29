# OoBDev.Testing.AppHost

Aspire host for the integration test stack. It starts the same services, images, host ports and credentials as
`containers/testing/docker-compose.integration-tests.yml`, so `src/.runsettings` works unchanged and
`dotnet test src/ --filter "TestCategory=Integration"` runs against either.

```bash
dotnet run --project src/Tools/OoBDev.Testing.AppHost      # dashboard on http://localhost:15889
dotnet test src/ --filter "TestCategory=Integration"
```

- Containers use a persistent lifetime and named volumes (`oobd-test-*`): stopping the host leaves them running, and the next run reuses them.
- Every host port is overridable with `TEST_PORT_<NAME>` (same names as the compose file); `SQL_SA_PASSWORD`, `OPENSEARCH_PASSWORD` and `OTEL_DASHBOARD_API_KEY` override the credentials.
- `moto-init` creates the SQS queues after Moto is healthy; the Service Bus emulator waits for SQL Server; nginx and OpenSearch Dashboards wait for their upstreams.
- sbert and ollama build from the repository Dockerfiles (phi3 is baked into the Ollama image), so the first start is slow.
- Do not run the compose stack and this host together: they use the same host ports. Use one or the other.
- `OoBDev.Example.AppHost` is the example application host; this one is only the test stack.
