using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>
/// A live display control (indicator, bar graph, strip chart, vector) as the view sees it: the
/// <see cref="LiveDisplayState"/> plus a <see cref="Revision"/> that changes with each published value,
/// so a view redraws from <see cref="State"/> on that notification.
/// </summary>
public sealed class LiveDisplayViewModel : ViewModelBase
{
    private int _revision;

    /// <summary>Creates the view model for <paramref name="control"/>.</summary>
    /// <param name="control">An indicator, bar graph, strip chart or vector control.</param>
    /// <exception cref="NotSupportedException">The control is not a live display control.</exception>
    public LiveDisplayViewModel(UiControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        Control = control;
        State = LiveDisplayState.For(control) ?? throw new NotSupportedException($"'{control.GetType().Name}' is not a live display control.");
        Bars = control is BarGraphControl bar ? [.. bar.Channels.Select((c, i) => new BarViewModel(c, ChartPalette.ColorFor(c, i)))] : [];
    }

    /// <summary>Whether this shows an indicator (<see cref="Text"/>).</summary>
    public bool IsIndicator => State is IndicatorState;

    /// <summary>Whether this shows a bar graph (<see cref="Bars"/>).</summary>
    public bool IsBarGraph => State is BarGraphState;

    /// <summary>The bars of a bar graph; empty for the other kinds.</summary>
    public IReadOnlyList<BarViewModel> Bars { get; }

    /// <summary>An indicator's display text, or null before a value arrives and for the other kinds.</summary>
    public string? Text => (State as IndicatorState)?.Text;

    /// <summary>The control definition.</summary>
    public UiControl Control { get; }

    /// <summary>The label text.</summary>
    public string Label => Control.Label;

    /// <summary>The current values; read it when <see cref="Revision"/> changes.</summary>
    public LiveDisplayState State { get; }

    /// <summary>Counts the published updates that changed the state.</summary>
    public int Revision
    {
        get => _revision;
        private set => SetProperty(ref _revision, value);
    }

    /// <summary>Publishes one value; returns whether the state changed.</summary>
    public bool Apply(string id, string text) => ApplyAll([new KeyValuePair<string, string>(id, text)]);

    /// <summary>Publishes several values as one update; returns whether the state changed.</summary>
    public bool ApplyAll(IEnumerable<KeyValuePair<string, string>> values)
    {
        if (!State.ApplyAll(values))
        {
            return false;
        }

        if (State is BarGraphState bars)
        {
            foreach (var bar in Bars)
            {
                bar.Update(bars);
            }
        }

        Revision++;
        OnPropertyChanged(nameof(State));
        OnPropertyChanged(nameof(Text));
        return true;
    }
}
