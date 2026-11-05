namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the routing window for one session.</summary>
public interface IRoutingDialog
{
    /// <summary>Shows the window modally.</summary>
    /// <param name="routing">The window view model.</param>
    void Show(RoutingDialogViewModel routing);
}
