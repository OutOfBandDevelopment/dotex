namespace OoBDev.ComplexEvents;

/// <summary>What a restarted scheduler does with occurrences it missed while it was not running.</summary>
public enum CatchUpPolicy
{
    /// <summary>Skip the missed occurrences and wait for the next one.</summary>
    Skip,

    /// <summary>Run only the most recent missed occurrence.</summary>
    RunLatest,

    /// <summary>Run every missed occurrence.</summary>
    RunAll,
}
