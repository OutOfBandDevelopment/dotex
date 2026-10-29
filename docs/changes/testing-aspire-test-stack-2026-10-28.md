# Aspire host for the integration test stack, and Unit test re-categorisation

**Date:** 2026-10-28
**Status:** Verified: AppHost started, Integration suite run against it

## What changed

- New `src/Tools/OoBDev.Testing.AppHost` (Aspire 13.6.1) mirrors `containers/testing/docker-compose.integration-tests.yml`: the same images, host ports (`TEST_PORT_<NAME>` overrides), credentials and volumes, so `src/.runsettings` works unchanged. `moto-init` runs after Moto is healthy, the Service Bus emulator waits for SQL Server, nginx and OpenSearch Dashboards wait for their upstreams, sbert and ollama build from the repository Dockerfiles. Containers are persistent. Readme: `README.Testing.AppHost.md`; `containers/testing/README.md` documents starting with Aspire.
- `HtmlToPdfConversionHandlerTests.ConvertAsyncTest` (runs the host wkhtmltopdf binary) is now `DevLocal` instead of `Unit`.
- `IDocumentConversionTests` Integration cases read `TIKA_URL` through `TestContext` instead of a hard-coded `127.0.0.1:9998`.

## Audit result

The whole Unit suite was run with only the plantuml container up. Everything passed except `DocsTests.Links_Relative_Resolve` (a link to the pending `templates/README.md` rename, not related). Heuristic scan of Unit files for processes, network addresses, file I/O and delays found only the two items above that really leave the process; the file, delay and mocked-client tests are in-process and stay `Unit`.

- Redis is served by Valkey 8 (`valkey/valkey:8-alpine`, BSD-3), a drop-in for the Redis protocol; service and container names (`redis`, `oobd-test-redis`) and `REDIS_*` settings are unchanged.

## Verification

The AppHost was started (20 resources, images pulled, sbert and ollama built) and `dotnet test --filter TestCategory=Integration` run against it. Fixes found by the run:

- Redis is a plain container: Aspire's `AddRedis` enables TLS and a password, which the tests do not use.
- Aspire binds host ports to 127.0.0.1 only, so SqlClient waited out `::1` on `localhost`. `SQLSERVER_CONNECTION_STRING` and `SQLSERVER_HOST` in `src/.runsettings` now use 127.0.0.1 (works with compose too).
- `OllamaApiClientTests.ListModelsTest` passed an empty model name that options validation rejects; it now uses `OLLAMA_MODEL`.

Result after the fixes: all Integration tests pass. The MongoDB failures (6) were a sealed `MongoDispatchProxy` (`DispatchProxy` rejects sealed bases); unsealed, all 6 pass.
