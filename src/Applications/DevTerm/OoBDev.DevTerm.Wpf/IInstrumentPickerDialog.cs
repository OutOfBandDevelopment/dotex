namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the instrument picker.</summary>
public interface IInstrumentPickerDialog
{
    /// <summary>Shows the picker modally.</summary>
    /// <param name="picker">The picker view model.</param>
    /// <returns>True when a choice was made.</returns>
    bool Pick(InstrumentPickerDialogViewModel picker);
}
