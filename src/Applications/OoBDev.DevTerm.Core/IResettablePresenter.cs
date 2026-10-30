namespace OoBDev.DevTerm;

/// <summary>A presenter that keeps state between renders (a partial line) and must drop it when its session reopens.</summary>
public interface IResettablePresenter
{
    /// <summary>Clears any state left over from a previous connection.</summary>
    void Reset();
}
