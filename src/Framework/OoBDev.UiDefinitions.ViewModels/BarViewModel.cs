using System.Globalization;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>One bar of a bar graph: its label, colour, fill fraction and value text.</summary>
public sealed class BarViewModel : ViewModelBase
{
    private double _fraction;
    private string _valueText = string.Empty;

    /// <summary>Creates the bar.</summary>
    /// <param name="channel">The channel it shows.</param>
    /// <param name="color">The channel colour as a hex string.</param>
    public BarViewModel(ChartChannel channel, string color)
    {
        Id = channel.Id;
        Label = channel.Label ?? channel.Id;
        Color = color;
    }

    /// <summary>The channel id.</summary>
    public string Id { get; }

    /// <summary>The label.</summary>
    public string Label { get; }

    /// <summary>The colour as a hex string.</summary>
    public string Color { get; }

    /// <summary>How full the bar is, 0 to 1.</summary>
    public double Fraction
    {
        get => _fraction;
        private set => SetProperty(ref _fraction, value);
    }

    /// <summary>The value with its unit, or empty before the first value.</summary>
    public string ValueText
    {
        get => _valueText;
        private set => SetProperty(ref _valueText, value);
    }

    internal void Update(BarGraphState state)
    {
        Fraction = state.FractionOf(Id);
        ValueText = state.ValueOf(Id) is { } value
            ? string.Create(CultureInfo.InvariantCulture, $"{value:0.##}{(state.Control.Unit is { Length: > 0 } unit ? " " + unit : string.Empty)}")
            : string.Empty;
    }
}
