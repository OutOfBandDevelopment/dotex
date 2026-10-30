using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Picks manifest editor files with the Windows file dialogs.</summary>
public sealed class WpfManifestFileDialogs : IManifestFileDialogs
{
    /// <inheritdoc />
    public string? PickManifestToOpen() =>
        Open("Device manifest (*.json)|*.json|All files (*.*)|*.*", "Open a device manifest to edit");

    /// <inheritdoc />
    public string? PickSavePath(string initialDirectory)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Device manifest (*.json)|*.json",
            FileName = "device.json",
            InitialDirectory = Directory.Exists(initialDirectory) ? initialDirectory : string.Empty,
        };
        return dialog.ShowDialog(Application.Current?.MainWindow) == true ? dialog.FileName : null;
    }

    /// <inheritdoc />
    public string? PickKsy() => Open("Kaitai Struct (*.ksy)|*.ksy|All files (*.*)|*.*", "Import a .ksy file as the binary frame");

    /// <inheritdoc />
    public string? PickRecording() => Open("dev-term session log (*.jsonl)|*.jsonl|All files (*.*)|*.*", "Use a session log as sample data");

    private static string? Open(string filter, string title)
    {
        var dialog = new OpenFileDialog { Filter = filter, Title = title };
        return dialog.ShowDialog(Application.Current?.MainWindow) == true ? dialog.FileName : null;
    }
}
