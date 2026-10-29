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
            Error = null;
        }
    }
}
