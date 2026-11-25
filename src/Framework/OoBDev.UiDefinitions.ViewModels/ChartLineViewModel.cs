using System.Globalization;
using System.Linq;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>One line of a strip chart, as a point list in a 100 by 100 box (origin top left) that a view stretches to its size.</summary>
public sealed class ChartLineViewModel : ViewModelBase
{
    private string _points = string.Empty;

    /// <summary>Creates the line.</summary>
    /// <param name="channel">The channel it plots.</param>
    /// <param name="color">The channel colour as a hex string.</param>
    public ChartLineViewModel(ChartChannel channel, string color)
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

    /// <summary>The points as <c>x,y x,y</c> text, which WPF reads as a point collection.</summary>
    public string Points
    {
        get => _points;
        private set => SetProperty(ref _points, value);
    }

    internal void Update(StripChartState state)
    {
        var samples = state.SamplesOf(Id);
        var (minimum, maximum) = state.Scale();
        var span = maximum - minimum;
        var step = state.Capacity > 1 ? 100d / (state.Capacity - 1) : 0;
        var offset = (state.Capacity - samples.Count) * step;
        Points = string.Join(' ', samples.Select((v, i) => string.Create(
            CultureInfo.InvariantCulture,
            $"{offset + (i * step):0.##},{100 - (global::System.Math.Clamp((v - minimum) / span, 0, 1) * 100):0.##}")));
    }
}
