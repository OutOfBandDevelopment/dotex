namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the Configure device window.</summary>
public interface IConfigureDeviceDialog
{
    /// <summary>Shows the window modally.</summary>
    /// <param name="device">The view model it edits.</param>
    void Show(ConfigureDeviceDialogViewModel device);
}
