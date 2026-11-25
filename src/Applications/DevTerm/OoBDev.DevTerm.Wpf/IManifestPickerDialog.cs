namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the device manifest picker.</summary>
public interface IManifestPickerDialog
{
    /// <summary>Shows the picker modally.</summary>
    /// <param name="picker">The picker view model; <see cref="ManifestPickerDialogViewModel.Chosen"/> is set when this returns true.</param>
    /// <returns>True when a manifest was chosen and loaded.</returns>
    bool Pick(ManifestPickerDialogViewModel picker);
}
