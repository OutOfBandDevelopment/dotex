namespace OoBDev.EventSourcing;

/// <summary>
/// Implemented by an aggregate that can save its state so loading does not replay the whole stream.
/// </summary>
public interface ISnapshotable
{
    /// <summary>
    /// Captures the current state as text (normally JSON).
    /// </summary>
    /// <returns>The state.</returns>
    string CaptureState();

    /// <summary>
    /// Restores the state captured by <see cref="CaptureState"/>.
    /// </summary>
    /// <param name="state">The state.</param>
    void RestoreState(string state);
}
