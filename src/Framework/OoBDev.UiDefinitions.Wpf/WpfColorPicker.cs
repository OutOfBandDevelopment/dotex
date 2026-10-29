using System.Windows;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Wpf;

/// <summary>Opens <see cref="ColorPickerWindow"/> as a modal dialog.</summary>
/// <param name="owner">The window the dialog centres on, or null.</param>
public sealed class WpfColorPicker(Window? owner = null) : IColorPicker
{
    /// <inheritdoc />
    public string? Pick(string? current)
    {
        var model = new ColorPickerViewModel(current);
        var window = new ColorPickerWindow(model) { Owner = owner };
        window.ShowDialog();
        return window.Accepted ? model.Result : null;
    }
}
