using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.WinForms;

/// <summary>Picks a colour with the standard Windows Forms <see cref="ColorDialog"/>; colours travel as <c>r,g,b</c> like the other renderers.</summary>
public sealed class WinFormsColorPicker : IColorPicker
{
    private readonly Func<Color, Color?> _show;

    /// <summary>Uses the real colour dialog, centred on <paramref name="owner"/> when given.</summary>
    public WinFormsColorPicker(IWin32Window? owner = null)
        : this(start =>
        {
            using var dialog = new ColorDialog { Color = start, FullOpen = true };
            return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.Color : null;
        })
    {
    }

    /// <summary>Uses <paramref name="show"/> in place of the dialog, so callers and tests need no UI.</summary>
    /// <param name="show">Receives the starting colour and returns the chosen one, or null for cancel.</param>
    public WinFormsColorPicker(Func<Color, Color?> show) => _show = show ?? throw new ArgumentNullException(nameof(show));

    /// <inheritdoc />
    public string? Pick(string? current)
    {
        var start = new ColorPickerViewModel(current);
        return _show(Color.FromArgb(start.Red, start.Green, start.Blue)) is { } chosen
            ? string.Create(CultureInfo.InvariantCulture, $"{chosen.R},{chosen.G},{chosen.B}")
            : null;
    }
}
