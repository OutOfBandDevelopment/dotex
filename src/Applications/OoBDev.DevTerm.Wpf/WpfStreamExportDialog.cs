using Microsoft.Win32;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Asks for the export path with a save file dialog.</summary>
public sealed class WpfStreamExportDialog : IStreamExportDialog
{
    /// <inheritdoc />
    public string? PickExportPath(string suggestedName, string extension, string kindName)
    {
        var dialog = new SaveFileDialog
        {
            FileName = suggestedName,
            DefaultExt = "." + extension,
            Filter = $"{kindName} (*.{extension})|*.{extension}|All files (*.*)|*.*",
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
