namespace OoBDev.WorkQueue;

/// <summary>
/// Counts of one stage. A supervisor decides whether a stage is wanted from <see cref="Claimable"/>, never from raw row counts.
/// </summary>
/// <param name="Claimable">Items a worker could claim right now.</param>
/// <param name="Running">Items under a claim.</param>
/// <param name="LeaseExpired">Running items whose lease or runtime ceiling has passed.</param>
/// <param name="Parked">Items parked because they hung.</param>
/// <param name="Dead">Items dead because they kept failing.</param>
public sealed record StageStats(int Claimable, int Running, int LeaseExpired, int Parked, int Dead);
