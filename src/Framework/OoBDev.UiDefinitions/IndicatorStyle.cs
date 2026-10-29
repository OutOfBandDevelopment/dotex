using System;
using System.Text.Json.Serialization;

namespace OoBDev.UiDefinitions;

/// <summary>How an <see cref="IndicatorControl"/> reads: plain, or as a warning (e.g. a "device not found" hint).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<IndicatorStyle>))]
public enum IndicatorStyle
{
    Plain,

    /// <summary>Drawn as a warning where the front end can (WPF: the window's warning text color; the TUI has no per-label color and shows it plain).</summary>
    Warning,
}
