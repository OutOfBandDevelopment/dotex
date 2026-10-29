# Aspire host for the integration test stack, and Unit test re-categorisation

**Date:** 2026-10-28
**Status:** Built; the AppHost was not started end to end (see Verification)

## What changed

- New `src/Tools/OoBDev.Testing.AppHost` (Aspire 13.6.1) mirrors `containers/testing/docker-compose.integration-tests.yml`: the same images, host ports (`TEST_PORT_<NAME>` overrides), credentials and volumes, so `src/.runsettings` works unchanged. `moto-init` runs after Moto is healthy, the Service Bus emulator waits for SQL Server, nginx and OpenSearch Dashboards wait for their upstreams, sbert and ollama build from the repository Dockerfiles. Containers are persistent. Readme: `README.Testing.AppHost.md`; `containers/testing/README.md` documents starting with Aspire.
- `HtmlToPdfConversionHandlerTests.ConvertAsyncTest` (runs the host wkhtmltopdf binary) is now `DevLocal` instead of `Unit`.
- `IDocumentConversionTests` Integration cases read `TIKA_URL` through `TestContext` instead of a hard-coded `127.0.0.1:9998`.

## Audit result

The whole Unit suite was run with only the plantuml container up. Everything passed except `DocsTests.Links_Relative_Resolve` (a link to the pending `templates/README.md` rename, not related). Heuristic scan of Unit files for processes, network addresses, file I/O and delays found only the two items above that really leave the process; the file, delay and mocked-client tests are in-process and stay `Unit`.

## Verification

- `dotnet build` of the AppHost: 0 errors (existing NU1902/NU1903 package advisories remain).
- Not run: the AppHost itself and the Integration suite against it (needs pulling about 20 images and building the Ollama image).
