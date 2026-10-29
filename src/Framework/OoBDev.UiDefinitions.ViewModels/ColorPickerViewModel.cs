using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>The state of a red, green and blue colour dialog.</summary>
public sealed class ColorPickerViewModel : ViewModelBase
{
    private int _red;
    private int _green;
    private int _blue;

    /// <summary>Creates the dialog state.</summary>
    /// <param name="current">The starting colour as <c>r,g,b</c>; anything else starts at black.</param>
    public ColorPickerViewModel(string? current)
    {
        var parts = (current ?? string.Empty).Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length == 3 && parts.All(p => byte.TryParse(p, NumberStyles.None, CultureInfo.InvariantCulture, out _)))
        {
            _red = byte.Parse(parts[0], CultureInfo.InvariantCulture);
            _green = byte.Parse(parts[1], CultureInfo.InvariantCulture);
            _blue = byte.Parse(parts[2], CultureInfo.InvariantCulture);
        }

        Accept = new AcceptColorCommand(this);
        Cancel = new CancelColorCommand(this);
    }

    /// <summary>Raised when the dialog should close; the argument is true when the user accepted.</summary>
    public event EventHandler<bool>? CloseRequested;

    /// <summary>The red channel, 0 to 255.</summary>
    public int Red
    {
        get => _red;
        set => SetChannel(ref _red, value);
    }

    /// <summary>The green channel, 0 to 255.</summary>
    public int Green
    {
        get => _green;
        set => SetChannel(ref _green, value);
    }

    /// <summary>The blue channel, 0 to 255.</summary>
    public int Blue
    {
        get => _blue;
        set => SetChannel(ref _blue, value);
    }

    /// <summary>The colour as a hex string, for a preview swatch.</summary>
    public string Preview => string.Create(CultureInfo.InvariantCulture, $"#{_red:X2}{_green:X2}{_blue:X2}");

    /// <summary>The colour as <c>r,g,b</c>, the same form <see cref="IColorPicker"/> returns.</summary>
    public string Result => string.Create(CultureInfo.InvariantCulture, $"{_red},{_green},{_blue}");

    /// <summary>Accepts the colour and closes.</summary>
    public ICommand Accept { get; }

    /// <summary>Closes without a colour.</summary>
    public ICommand Cancel { get; }

    internal void Close(bool accepted) => CloseRequested?.Invoke(this, accepted);

    private void SetChannel(ref int field, int value)
    {
        if (SetProperty(ref field, global::System.Math.Clamp(value, 0, 255)))
        {
            OnPropertyChanged(nameof(Preview));
            OnPropertyChanged(nameof(Result));
        }
    }
}
