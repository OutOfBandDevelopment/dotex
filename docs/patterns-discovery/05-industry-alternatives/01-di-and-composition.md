# DI and Composition

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Index](./README.md) · [Project Structure and Build →](./02-project-structure-and-build.md)
<!-- nav -->

## 1. Provider selection

**Today:** default plus keyed registration and `ISelectedService<T>` reading `OoBDev::ServiceKeys::{FullTypeName}` ([pattern 4](../02-design-patterns/04-selected-service.md)).

**Table 1 — Provider selection options**

| Option | Pros | Cons |
|--------|------|------|
| Current: default + keyed + `ISelectedService<T>` | Config-only switching; default exists without config; one wrapper type for every capability | Selection resolved in a constructor; key path is a convention nobody validates; extra indirection for consumers |
| Keyed services alone (`[FromKeyedServices]`) | Built into `Microsoft.Extensions.DependencyInjection` since .NET 8; no wrapper | The key is chosen in code at the consumer, not by configuration |
| Named options plus a factory | Familiar `IOptionsMonitor` model; reload support | Options is not a service locator; boilerplate per capability |
| Third-party container (Autofac, Lamar) | Modules, decorators, richer resolution | Extra dependency; the built-in container is now good enough for this design |
| Feature flags (`Microsoft.FeatureManagement`) | Runtime toggles, targeting, gradual rollout | Solves a different problem than provider choice |

**Verdict: Keep.** It matches what the platform now offers (keyed services) while adding configuration selection. **Consider** reading the key through an options type and validating at startup that the key names a registered provider.

## 2. Options binding and validation

**Today:** `Configure<T>(o => configuration.Bind(section, o))`, `IOptions<T>`, no validation ([pattern 7](../02-design-patterns/07-options-binding-by-section-name.md)).

**Table 2 — Options approaches**

| Option | Pros | Cons |
|--------|------|------|
| Current | Simple; section name overridable via builder | Misconfiguration surfaces late as null references; no startup feedback |
| `AddOptions<T>().BindConfiguration(section).ValidateDataAnnotations().ValidateOnStart()` | Fails fast at startup with a clear message; same section-name approach | Small amount of per-options code; annotations need discipline |
| Source-generated validators (`[OptionsValidator]`) | No reflection, AOT-friendly | More types to maintain |
| `IOptionsMonitor<T>` | Live reload | More moving parts; most options here are read once |

**Verdict: Change.** Adopt `BindConfiguration` plus `ValidateOnStart` in new capabilities; keep the section-name builder records.

## 3. `#if DEBUG` required parameters

**Today:** optional builder parameters are required in Debug builds ([pattern 9](../02-design-patterns/09-if-debug-explicit-arguments.md)). The purpose is deliberate: roll-up methods must forward each child builder to the layer beneath them, and a default in dev builds would make it easy to miss a caller that forgets to pass it.

**Table 3 — Alternatives**

| Option | Pros | Cons |
|--------|------|------|
| Current | A missed builder is a compile error while developing; short calls in Release | API surface differs by configuration; a Debug-built library breaks Release callers; can confuse tooling |
| Always-required parameter | Same API everywhere; same safety | Every consumer must pass `null` or a builder, even for one-liners |
| Analyzer that reports calls omitting the builder | One API, safety kept in CI, one-liners possible | Needs writing and maintaining |
| `Action<TBuilder>` delegate | The common .NET idiom | Does not force forwarding; not immutable-record friendly |

**Verdict: Keep.** It solves a real forwarding problem cheaply. The cost is that the whole solution must be built in one configuration. **Consider** an analyzer if the configuration-dependent API ever causes friction for external consumers.

## 4. Builder records vs configure delegates

**Table 4 — Configuration objects**

| Option | Pros | Cons |
|--------|------|------|
| Current: `record` builder with section names | Immutable; object-initializer syntax; easy to diff; composable per layer | Unusual for .NET developers; not reusable across DI features that expect delegates |
| `Action<TOptions>` delegates | Standard and expected | Mutable; harder to inspect |
| Options classes bound from configuration only | No code needed | No place for switches such as `Disable…` |

**Verdict: Keep.** Optionally add an `Action<TBuilder>` overload for people who expect it.

## 5. `IServiceProvider` injection

**Today:** used inside `SelectedService<T>` and factories only.

**Table 5 — Service-locator concerns**

| Option | Pros | Cons |
|--------|------|------|
| Current: contained in wrappers and factories | Enables runtime selection | Hides dependencies if it spreads |
| Pure constructor injection everywhere | Explicit dependencies | Cannot choose among keyed services at runtime |
| `IServiceScopeFactory` for scoped work | Correct lifetime handling | More code |

**Verdict: Keep, but contained.** Add a review rule that only infrastructure classes may take `IServiceProvider`.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Index](./README.md) · [Project Structure and Build →](./02-project-structure-and-build.md)
<!-- nav -->
