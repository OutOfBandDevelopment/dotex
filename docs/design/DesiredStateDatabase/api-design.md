# Desired-State Database Tooling — API design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

**Status:** Draft (shape only; fixed after phase 0) · **Date:** 2026-10-29

## Built shape (2026-11-23)

The sketch below became `IDesiredStateProvider.PlanAsync(DesiredSchema, DatabaseTarget)` and `ApplyAsync(desired, target, plan, ApplyOptions)`; `ApplyOptions` has `AllowDataLoss` and `AllowTypeChange`, a plan with an unpermitted hazard throws `UnsafePlanException`, and providers are keyed by engine and resolved by `DesiredStateProviderFactory`. Ensure scripts run through `EnsureScriptRunner` (not an interface): file-name order, idempotent scripts every time, `*.once.sql` once per recorded hash through `IAppliedScriptStore`.

## Sketch

```csharp
public interface IDesiredStateProvider
{
    string Engine { get; }                                   // "sqlserver", "postgres", "sqlite"
    Task<SchemaPlan> PlanAsync(DesiredState desired, string connectionString, CancellationToken ct);
    Task ApplyAsync(SchemaPlan plan, ApplyOptions options, CancellationToken ct);   // refuses hazards unless allowed
}

public sealed record SchemaPlan(IReadOnlyList<PlanStep> Steps, IReadOnlyList<Hazard> Hazards);
public sealed record ApplyOptions(bool AllowDataLoss = false, bool AllowLocks = false);
```

Providers follow the provider/factory pattern: `IDesiredStateProviderFactory` resolves by engine key, registration uses `TryAdd*`. The CLI (`OoBDev.DacPacCompiler.Cli` or a sibling) exposes `plan` and `apply` verbs. Ensure scripts run through a separate `IEnsureScriptRunner` that orders by file name and records applied hashes only when the script is not idempotent by itself.
