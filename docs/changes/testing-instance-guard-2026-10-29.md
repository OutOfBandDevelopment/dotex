# Integration test guard (2026-10-29)

`OoBDev.TestUtilities.TestInstanceGuard` (`IsTestInstance`, `Require`) refuses to run an Integration test whose connection string or URL names a host other than loopback or `host.docker.internal`. Unit tests: 13 in `OoBDev.TestUtilities.Tests`. `SqlServerDeploymentTests` now reads its connection string through it. Other Integration tests can adopt it as they are touched.

Also: the `templates/` readmes were renamed to `ReadMe.Templates.md` and `ReadMe.AppName.md`; the package and the patterns link follow, and `dotnet pack` was verified.

## Adoption (later 2026-10-29)

- `TestInstanceGuard` now also reads StackExchange.Redis style strings (`host:6379,password=x`), and is applied in the MongoDB Integration tests and the Redis Integration and DevLocal tests. Verified against the running stack (Mongo 6 tests, Redis 10 tests passed).
- Also applied to the URL based Integration tests (Tika, Ollama, OpenSearch, SBert, Keycloak, SQS, Documents, LlmCodeGen); all pass against the stack (SBert: 2 skipped as before). Not applied: Service Bus (emulator string `Endpoint=sb://...` needs its own parsing), the OTLP tests.
