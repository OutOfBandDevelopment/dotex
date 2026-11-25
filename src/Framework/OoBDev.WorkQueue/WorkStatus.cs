namespace OoBDev.WorkQueue;

/// <summary>
/// Status values of a work item, as stored in the task row.
/// </summary>
public static class WorkStatus
{
    /// <summary>Waiting to be claimed (possibly until its retry time).</summary>
    public const string Pending = "pending";

    /// <summary>Claimed by a worker under a lease.</summary>
    public const string Running = "running";

    /// <summary>Finished.</summary>
    public const string Done = "done";

    /// <summary>Finished without work (nothing to do).</summary>
    public const string Skipped = "skipped";

    /// <summary>Taken out of the flow because it kept hanging; can be redriven.</summary>
    public const string Parked = "parked";

    /// <summary>Taken out of the flow because it kept failing; can be redriven.</summary>
    public const string Dead = "dead";
}
