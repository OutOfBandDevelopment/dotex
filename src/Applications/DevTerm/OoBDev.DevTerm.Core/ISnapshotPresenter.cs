namespace OoBDev.DevTerm;

/// <summary>
/// A presenter whose state between renders (a partial line, a half-read frame) can be captured and put back, so
/// playback can seek backward from a log checkpoint instead of replaying every record from the start. A presenter with no
/// state returns <see langword="null"/> and ignores <see cref="Restore"/>. A pipeline can only restore when every presenter in it implements this.
/// </summary>
public interface ISnapshotPresenter : IPresenter
{
    /// <summary>Captures the state after the last <see cref="IPresenter.Render"/>. The result is immutable or private: later renders must not change it.</summary>
    object? Snapshot();

    /// <summary>Puts back state captured by <see cref="Snapshot"/> on a presenter of the same kind.</summary>
    void Restore(object? snapshot);
}
