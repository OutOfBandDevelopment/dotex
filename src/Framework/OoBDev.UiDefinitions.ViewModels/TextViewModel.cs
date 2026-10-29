using System.Globalization;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>A text, numeric or slider row: the value is edited as text and validated against the control's constraint.</summary>
public sealed class TextViewModel : ControlViewModel
{
    private string _text = string.Empty;

    /// <summary>Creates the view model and reads the model.</summary>
    /// <param name="control">A text field, numeric or slider control.</param>
    /// <param name="binding">The bound model.</param>
    public TextViewModel(UiControl control, FormBinding binding)
        : base(control, binding)
    {
        Refresh();
    }

    /// <summary>Whether this row is a slider, so a view can offer a slider widget beside the text.</summary>
    public bool IsSlider => Control is SliderControl;

    /// <summary>The slider minimum, or 0 when this is not a slider.</summary>
    public double Minimum => (Control as SliderControl)?.Minimum ?? 0;

    /// <summary>The slider maximum, or 0 when this is not a slider.</summary>
    public double Maximum => (Control as SliderControl)?.Maximum ?? 0;

    /// <summary>The slider step, or 1 when this is not a slider.</summary>
    public double Step => (Control as SliderControl)?.Step ?? 1;

    /// <summary>The slider value as a number; setting it writes the model through <see cref="Text"/>.</summary>
    public double Value
    {
        get => double.TryParse(_text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : Minimum;
        set => Text = value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>The value as text; setting it validates and writes the model, and <see cref="ControlViewModel.Error"/> reports a rejection.</summary>
    public string Text
    {
        get => _text;
        set
        {
            if (!SetProperty(ref _text, value ?? string.Empty))
            {
                return;
            }

            OnPropertyChanged(nameof(Value));
            var result = Binding.SetText(Control, _text);
            Error = result.IsValid ? null : result.Error;
        }
    }

    /// <inheritdoc />
    protected override void RefreshValue()
    {
        var current = Binding.GetText(Id);
        if (SetProperty(ref _text, current, nameof(Text)))
        {
            OnPropertyChanged(nameof(Value));
            Error = null;
        }
    }
}
