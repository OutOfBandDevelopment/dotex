using System.Collections.Generic;

namespace OoBDev.Data.DesiredState;

/// <summary>What a package deployment did.</summary>
/// <param name="RefactorsRun">Ids of the refactor entries that ran.</param>
/// <param name="RefactorsRecorded">Ids recorded without running because the table did not exist yet.</param>
/// <param name="Plan">The schema plan that was applied.</param>
/// <param name="PreDeploy">Report of the pre-deploy scripts.</param>
/// <param name="PostDeploy">Report of the post-deploy scripts.</param>
/// <param name="FirstDeploy">True when this package hash was not recorded before.</param>
public sealed record PackageDeployResult(
    IReadOnlyList<string> RefactorsRun,
    IReadOnlyList<string> RefactorsRecorded,
    SchemaPlan Plan,
    EnsureReport PreDeploy,
    EnsureReport PostDeploy,
    bool FirstDeploy);
