namespace OoBDev.Data.DesiredState;

/// <summary>What <see cref="EfDesiredStateDeployer"/> did for one context.</summary>
/// <param name="Plan">The plan that was computed (empty when the database already matched the model).</param>
/// <param name="Applied">True when statements were run.</param>
/// <param name="Ensure">The ensure-script report, or <c>null</c> when no ensure scripts were given.</param>
public sealed record EfDesiredStateResult(SchemaPlan Plan, bool Applied, EnsureReport? Ensure);
