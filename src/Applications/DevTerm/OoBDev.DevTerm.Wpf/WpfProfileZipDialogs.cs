using System.Windows;
using Microsoft.Win32;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Picks the profile zip with the Windows file dialogs.</summary>
public sealed class WpfProfileZipDialogs : IProfileZipDialogs
{
    private const string Filter = "Connection profiles (*.zip)|*.zip|All files (*.*)|*.*";

    /// <inheritdoc />
    public string? PickZipToImport()
    {
        var dialog = new OpenFileDialog { Filter = Filter, Title = "Import connection profiles" };
        return dialog.ShowDialog(Application.Current?.MainWindow) == true ? dialog.FileName : null;
    }

    /// <inheritdoc />
    public string? PickZipToExport()
    {
        var dialog = new SaveFileDialog { Filter = Filter, FileName = "profiles.zip", Title = "Export connection profiles" };
        return dialog.ShowDialog(Application.Current?.MainWindow) == true ? dialog.FileName : null;
    }
}
