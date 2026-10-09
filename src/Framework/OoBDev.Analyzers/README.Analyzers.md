# OoBDev.Analyzers

Small Roslyn analyzer that enforces OoBDev conventions at build time.

| Id | Rule | Severity |
|----|------|----------|
| OOB0001 | Use an injected `TimeProvider` instead of `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now` or `DateTimeOffset.UtcNow` | warning |
| OOB0002 | A constant string key passed to `AddKeyed*` or `TryAddKeyed*` must be lower-case kebab-case | warning |
| OOB0003 | Third-party IoC containers and logging libraries are not allowed (`Autofac`, `Serilog`, `NLog`, `log4net`, `Ninject`, `SimpleInjector`, `StructureMap`, `Castle.Windsor`, `Polly`) | warning |

Tune severities per project in `.editorconfig`, for example `dotnet_diagnostic.OOB0001.severity = none`.
