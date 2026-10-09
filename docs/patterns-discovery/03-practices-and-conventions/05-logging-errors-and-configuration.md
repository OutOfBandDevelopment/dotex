# Logging, Errors and Configuration

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← Testing Practices](./04-testing-practices.md) · [CI/CD and Versioning →](./06-cicd-and-versioning.md)
<!-- nav -->

## Logging

- `ILogger<T>` injected through the constructor; never a static logger.
- Message templates are written with `nameof` in braces to keep names in sync with variables, for example `$"{{{nameof(userName)}}} is authorized ({{{nameof(userId)}}})"` followed by the arguments.
- Missing optional configuration is a `LogWarning` and the feature quietly turns off ([config-gated registration](../02-design-patterns/08-config-gated-registration.md)).
- Background loops log the exception and continue ([supervised hosted service](../02-design-patterns/13-supervised-hosted-service.md)).

## Errors

- Configuration that must exist fails with `ConfigurationMissingException` at first use.
- Business outcomes travel in the result envelope (`IResult`, `ResultMessage`), see [pattern 15](../02-design-patterns/15-result-envelope.md); infrastructure failures throw.
- Cross-cutting decorators must not change behavior on failure: the caching proxy swallows and logs in Release, but rethrows in Debug (`#if DEBUG`).

## Configuration

**Table 6 — Configuration conventions**

| Rule | Detail |
|------|--------|
| Section names | default to `nameof(TheOptionsType)`, overridable through a builder record |
| Provider selection | a configuration path per capability, for example `OoBDev:CachingProvider:Type` (see [config-selected keyed service](../02-design-patterns/04-selected-service.md)) |
| Message routing | `MessageQueue:{Channel}:{Message}` down to `MessageQueue:Default` ([pattern 5](../02-design-patterns/05-config-resolved-provider.md)) |
| Feature switches | `Disable…` booleans in builder records, `OoBDev:Caching:Disabled` style keys |
| Environment switches | `IDENTITY_PROVIDER`, `SWAGGER_ONLY` in the example app |
| Secrets | never in the repo; supplied by environment, user secrets or `.runsettings` overrides |
| Reference | all keys documented in `CONFIGURATION_SETTINGS.md` |

## Time, identity and randomness

Use the injectable providers (`TimeProvider`, `IGuidProvider`, `ICurrentUserAccessor`, `ITempFileFactory`) rather than `DateTime.Now`, `Guid.NewGuid()` or ambient identity, so behavior is testable ([pattern 16](../02-design-patterns/16-injectable-non-determinism.md)).

---

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← Testing Practices](./04-testing-practices.md) · [CI/CD and Versioning →](./06-cicd-and-versioning.md)
<!-- nav -->
