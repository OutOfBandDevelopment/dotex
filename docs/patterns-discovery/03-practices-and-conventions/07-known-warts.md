# Known Warts (Decide Before Copying)

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← CI/CD and Versioning](./06-cicd-and-versioning.md) · [UI Practices (MVVM and Command Binding) →](./08-ui-practices.md)
<!-- nav -->

Things found in the code that a new project should either fix or knowingly keep. Each is compared with industry practice in [Industry Alternatives](../05-industry-alternatives/README.md).

**Table 8 — Known warts**

| # | Wart | Where | Suggested handling |
|---|------|-------|--------------------|
| 1 | `RetrieveAsync` misspelled in the public API | caching abstractions | fixed 2026-10-09 (renamed outright; the framework is unreleased) |
| 2 | `ServiceCollectionEx` vs `ServiceCollectionExtensions` | six projects | Done 2026-10-09: all use `ServiceCollectionExtensions` |
| 3 | Sync-over-async (`GetAwaiter().GetResult()`) in the caching proxy | `CachedProxy` | async-aware proxy or decorator |
| 4 | Caller info from `new StackFrame(5, true)` | message sender | fixed 2026-10-09: `[CallerMemberName]` and friends on `SendAsync` |
| 5 | `[ContractConfig]` declared but never read | `ICachingProvider` | removed 2026-10-09 with `ISelectedService<T>`; the path is `CachingGlobals.ConfigurationPath` |
| 6 | Readme case (was `Readme.X.md`/`ReadMe.X.md` on disk, `README.X.md` in props) | shared props | **Resolved:** all project readmes renamed to `README.X.md`; the old names only worked on case-insensitive file systems |
| 7 | `#if DEBUG` changes the compiled API (intentional: forces child builders to be forwarded) | registration entry points | keep; build one configuration; see alternatives |
| 8 | No options validation, no `ValidateOnStart` | all options | add validation |
| 9 | Central package management off; versions inline | every csproj | fixed: central `Directory.Packages.props` is on (verified 2026-10-09; only `Incoming/` code keeps inline versions) |
| 10 | Analyzers and XML-doc generation commented out | shared props | partly fixed: `OoBDev.Analyzers` (OOB0001 to OOB0003) is wired into every project; stock analyzers and XML-doc generation still off, enable and gate |
| 11 | Provider key casing differs (`Redis`, `OLLAMA`, `rabbit-mq`) | adapters | constants on the abstraction |
| 12 | MD5 as a default hash | `OoBDev.System` | prefer SHA-256 for anything security-adjacent |
| 13 | Hard-coded 10 second restart delay | `MessageReceiverHost` | make configurable |

Doc/code differences (for example the `GitVersion.yml` location) are tracked in [Doc/Code Drift](../README.md).

---

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← CI/CD and Versioning](./06-cicd-and-versioning.md) · [UI Practices (MVVM and Command Binding) →](./08-ui-practices.md)
<!-- nav -->
