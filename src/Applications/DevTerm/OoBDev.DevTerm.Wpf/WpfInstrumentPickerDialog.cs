using System.Windows;
using DevTermApp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows an <see cref="InstrumentPickerDialogViewModel"/> in an <see cref="InstrumentPickerWindow"/>.</summary>
public sealed class WpfInstrumentPickerDialog : IInstrumentPickerDialog
{
    /// <inheritdoc />
    public bool Pick(InstrumentPickerDialogViewModel picker) =>
        new InstrumentPickerWindow(picker) { Owner = Application.Current?.MainWindow }.ShowDialog() == true;
}
