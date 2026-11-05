using System.Linq;
using System.Windows;
using DevTermApp;
using OoBDev.DevTerm.DeviceManifests.Editing;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows an <see cref="ExpressionPickerViewModel"/> in an <see cref="ExpressionPickerWindow"/>.</summary>
public sealed class WpfExpressionPickerDialog : IExpressionPickerDialog
{
    /// <inheritdoc />
    public string? Pick(ExpressionPickerViewModel picker)
    {
        var window = new ExpressionPickerWindow(picker) { Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current?.MainWindow };
        return window.ShowDialog() == true ? window.Accepted : null;
    }
}
