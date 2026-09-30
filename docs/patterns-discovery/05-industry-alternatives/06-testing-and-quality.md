# Testing and Quality

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Data Access and HTTP API](./05-data-and-api.md) · [Documentation →](./07-documentation.md)
<!-- nav -->

## 20. Test framework and mocking

**Today:** MSTest 4, Moq in strict mode, five test categories ([testing practices](../03-practices-and-conventions/04-testing-practices.md)).

**Table 20 — Test tooling**

| Option | Pros | Cons |
|--------|------|------|
| Current MSTest + Moq | Familiar; category filters and `TestContext` integrate with `.runsettings` | Moq had a dependency-privacy controversy in 2023 (a build-time analyzer reading local data, later removed); some teams pin versions |
| xUnit | Very common; constructor-based setup; parallel by default | Different lifecycle; no `TestContext` equivalent for run parameters |
| NUnit | Rich constraints and parameterized tests | Another style to learn |
| NSubstitute or FakeItEasy | Lighter syntax than Moq | Behavior differs from strict-mode expectations |
| Hand-written fakes | No library | More code |
| `Microsoft.Testing.Platform` runner | Modern, fast runner supported by MSTest | Needs runner migration |

**Verdict: Consider.** MSTest is a good fit because `.runsettings` and `TestContext` drive configuration. Re-evaluate Moq against NSubstitute at the next major upgrade, and prefer fakes for stable, simple interfaces.

**Owner decision:** not migrating from MSTest. Other mocking frameworks may be considered but only after spikes.

## 21. Docker test infrastructure

**Today:** a compose stack with 15 services, started by scripts before the tests ([testing practices](../03-practices-and-conventions/04-testing-practices.md)).

**Table 21 — Container strategies**

| Option | Pros | Cons |
|--------|------|------|
| Current docker-compose | One shared stack; usable by hand and by CI; supports heavy services (Ollama, OpenSearch) | Tests depend on external state; start-up ordering and health checks are scripts; global port use |
| Testcontainers for .NET | Containers created per test class from code; automatic cleanup; random ports | Slower start per fixture; needs a Docker API |
| .NET Aspire testing | Composes services with the app model | Adds the Aspire model |
| Service containers in CI | Simple in GitHub Actions | Not usable locally |
| Emulators only (Azurite, LocalStack) | Light | Only some services |

**Verdict: Consider.** Keep compose for the shared long-running stack; use Testcontainers for new, self-contained tests so a single test project can run without the whole stack.

**Owner decision:** .NET Aspire did not exist when this was built and may be adopted as long as no functionality is lost; a spike should prove that.

## Coverage and mutation

`coverlet` collects coverage. **Consider** adding mutation testing (Stryker.NET) for the core capabilities where the 80 percent line-coverage target hides weak assertions.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Data Access and HTTP API](./05-data-and-api.md) · [Documentation →](./07-documentation.md)
<!-- nav -->
