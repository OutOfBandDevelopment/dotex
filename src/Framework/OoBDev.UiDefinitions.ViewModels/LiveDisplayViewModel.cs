using System;
using System.Collections.Generic;

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
    }

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

        Revision++;
        return true;
    }
}
