using System.Windows;
using DevTermApp;
using Microsoft.Win32;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Picks a log with the file dialog and shows it in a <see cref="PlaybackWindow"/>.</summary>
public sealed class WpfPlaybackDialog : IPlaybackDialog
{
    /// <inheritdoc />
    public string? PickLog()
    {
        var dialog = new OpenFileDialog { Filter = "Session logs (*.jsonl)|*.jsonl|All files (*.*)|*.*" };
        return dialog.ShowDialog(Application.Current?.MainWindow) == true ? dialog.FileName : null;
    }

    /// <inheritdoc />
    public void Show(PlaybackDialogViewModel playback) =>
        new PlaybackWindow(playback) { Owner = Application.Current?.MainWindow }.ShowDialog();
}
