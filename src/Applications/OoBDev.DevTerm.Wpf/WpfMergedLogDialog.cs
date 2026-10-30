using System.Windows;
using DevTermApp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the merged log view model in a <see cref="MergedLogWindow"/>.</summary>
public sealed class WpfMergedLogDialog : IMergedLogDialog
{
    /// <inheritdoc />
    public void Show(MergedLogDialogViewModel log)
    {
        using (log)
        {
            new MergedLogWindow(log) { Owner = Application.Current?.MainWindow }.ShowDialog();
        }
    }
}
