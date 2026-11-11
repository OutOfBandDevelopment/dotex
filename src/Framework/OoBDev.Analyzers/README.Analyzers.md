# OoBDev.Analyzers

Small Roslyn analyzer that enforces OoBDev conventions at build time.

| Id | Rule | Severity |
|----|------|----------|
| OOB0001 | Use an injected `TimeProvider` instead of `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now` or `DateTimeOffset.UtcNow` | warning |
| OOB0002 | A constant string key passed to `AddKeyed*` or `TryAddKeyed*` must be lower-case kebab-case | warning |
| OOB0003 | Third-party IoC containers and logging libraries are not allowed (`Autofac`, `Serilog`, `NLog`, `log4net`, `Ninject`, `SimpleInjector`, `StructureMap`, `Castle.Windsor`; `Polly` is not banned because the accepted `Microsoft.Extensions.Resilience` API is built on its types) | warning |

Tune severities per project in `.editorconfig`, for example `dotnet_diagnostic.OOB0001.severity = none`.

## Decided: no DevTerm rule yet

Decided 2026-11-09: the checkable DevTerm constraints are legitimate in most uses or need data flow the analyzer cannot see, so a rule now would be noise. Add `OOB0004` only when a repeatable mistake appears.
