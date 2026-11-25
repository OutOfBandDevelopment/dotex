using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>A read-only row of a generated form: the model's current text, shown plain or as a warning.</summary>
public sealed class IndicatorViewModel : ControlViewModel
{
    private string _text = string.Empty;

    /// <summary>Creates the view model and reads the model.</summary>
    /// <param name="control">The indicator.</param>
    /// <param name="binding">The bound model.</param>
    public IndicatorViewModel(IndicatorControl control, FormBinding binding)
        : base(control, binding)
    {
        Indicator = control;
        Refresh();
    }

    /// <summary>The indicator definition.</summary>
    public IndicatorControl Indicator { get; }

    /// <summary>Whether a view should draw the text as a warning (the error colour).</summary>
    public bool IsWarning => Indicator.Style == IndicatorStyle.Warning;

    /// <summary>The model's current text, or the definition's default while the model has none.</summary>
    public string Text
    {
        get => _text;
        private set => SetProperty(ref _text, value);
    }

    /// <inheritdoc />
    protected override void RefreshValue() => Text = Binding.Has(Id) ? Binding.GetText(Id) : Indicator.DefaultValue ?? string.Empty;
}
