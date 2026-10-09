# Application Insights replaced by OpenTelemetry, broken DevLocal tests fixed

**Date:** 2026-10-09 · **Epic:** Observability and test health · **Status:** Complete

## Summary

The Application Insights project is now a plain OpenTelemetry project, `OoBDev.OpenTelemetry`, that exports over OTLP to any collector. The Azurinsight emulator, which could not read the 3.x exporter's payload, is replaced in the Docker test stack by Grafana LGTM, and the Integration tests read spans and logs back from Tempo and Loki. The same session fixed or re-categorized most of the `DevLocal` tests.

## Contents

- [OpenTelemetry replacement](#opentelemetry-replacement)
- [Test stack](#test-stack)
- [DevLocal test fixes](#devlocal-test-fixes)
- [Follow-up](#follow-up)

## OpenTelemetry replacement

**Table 1 — What changed**

| Item | Change |
|------|--------|
| `OoBDev.Microsoft.ApplicationInsights` and its tests | Moved (history kept) to `src/ExternalServices/OpenTelemetry/OoBDev.OpenTelemetry` and `.Tests`; namespace `OoBDev.OpenTelemetry`; the `Microsoft.ApplicationInsights` packages are removed from `Directory.Packages.props` |
| Registration | `TryAddOpenTelemetryExtensions(IConfiguration, sectionName)` replaces `TryAddApplicationInsightsExtensions()`. It adds the four processors, the providers and, when an endpoint is configured, the OTLP exporters. Config-gated: no section and no `OTEL_EXPORTER_OTLP_ENDPOINT` means nothing is added |
| `OpenTelemetryOptions` | `OtlpEndpoint`, `Protocol`, `ServiceName`, `Sources` ([readme](../../src/ExternalServices/OpenTelemetry/OoBDev.OpenTelemetry/README.OpenTelemetry.md)) |
| `TryCommonExternalExtensions` | Calls the new method in every build; it was a Debug-only call marked incomplete. `ExternalExtensionBuilder.OpenTelemetryOptionSection` names the section |
| Packages | `OpenTelemetry.Extensions.Hosting` and `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.19.1 (tests: `OpenTelemetry.Exporter.InMemory`) |

Two details worth knowing: an OTLP endpoint set in code is used as given, so for `HttpProtobuf` the library appends `v1/traces` and `v1/logs` (the environment variable gets this from the SDK); and the old class names were removed rather than forwarded, because the project and namespace moved. Nothing in the repository used them by name.

## Test stack

- `otel-lgtm` (`grafana/otel-lgtm`: OTLP on 4317 and 4318, Grafana on 3000, with Tempo, Loki and Prometheus) replaces `azurinsight` in `containers/testing/docker-compose.integration-tests.yml`, `.env.integration`, `src/.runsettings`, the CI workflow, start-up and wait scripts, the nginx dashboard and the stack readme.
- `OtlpIntegrationTests` (Integration): a span with a correlation tag reaches Tempo; a log record with the correlation attribute reaches Loki. Both pass against the container.
- 6 Unit tests cover the processors and the registration (no settings adds nothing; settings add processors and tag exported spans).

## DevLocal test fixes

- Unit: Documents registry pair (a mock `IBlobContainerProvider` and logging), `ReflectionElementNodeTest` (the test utility now writes XPath navigators as XML), Html `DeeperTest` (`SimpleCopy.xslt` embedded).
- Integration: the document conversion tests that need Tika (12 pass against the container) and the Example blob tests (Azurite; the tests create their containers).
- Kept `DevLocal` on purpose: Ollama, USB HID, `PathEx`, `MergedXPathNavigator`, `ProjectTools`, Markdown `TestMethod1`, DacFx `BuildPackageTest`.

## Azure B2C dropped

The `OoBDev.Microsoft.Azure.B2C` library and its tests are removed, along with `IdentityProviders.AzureB2C`, `IdentityExtensionBuilder.MicrosoftIdentityConfigurationSection`, the example host B2C profile and settings, the solution entries and the B2C test variables. `IdentityProvider` now defaults to `Keycloak`. This is a breaking public API removal made at the owner's request. `Microsoft.Graph` and `Azure.Identity` stay because DacFx still references them. The Epic 7 proposals under `Features/Proposals/07-Identity` and the historical change documents still mention B2C as design history.

## Follow-up

- The `oobdev/azurinsight` image and its fork are no longer used by this repository.
- `docs/generated`, `docs/Libraries` and `docs/code` still contain generated pages for the old project name; they regenerate with the next documentation build.

[↑ Change index](README.md)
