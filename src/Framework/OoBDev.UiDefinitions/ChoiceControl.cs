using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OoBDev.UiDefinitions;

/// <summary>One of a fixed set of named options (e.g. a color preset, a connection mode).</summary>
public sealed class ChoiceControl : UiControl
{
    public List<string> Options { get; set; } = [];

    public string? DefaultValue { get; set; }

    public ChoiceStyle Style { get; set; } = ChoiceStyle.Dropdown;
}
