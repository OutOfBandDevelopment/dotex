namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the stream monitor window.</summary>
public interface IStreamMonitorDialog
{
    /// <summary>Shows the window modally.</summary>
    /// <param name="monitor">The window view model.</param>
    void Show(StreamMonitorDialogViewModel monitor);
}
