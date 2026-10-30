namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the merged log window.</summary>
public interface IMergedLogDialog
{
    /// <summary>Shows the window modally and disposes <paramref name="log"/> when it closes.</summary>
    /// <param name="log">The window view model.</param>
    void Show(MergedLogDialogViewModel log);
}
