using System.Collections.Generic;

namespace OoBDev.UiDefinitions;

/// <summary>One of a fixed set of named options (e.g. a color preset, a connection mode).</summary>
public sealed class ChoiceControl : UiControl
{
    public List<string> Options { get; set; } = [];

    /// <summary>The model property that supplies <see cref="Options"/>, when the list can change while the form is open (a form view model re-reads it on every refresh); null for a fixed list.</summary>
    public string? OptionsSource { get; set; }

    public string? DefaultValue { get; set; }

    public ChoiceStyle Style { get; set; } = ChoiceStyle.Dropdown;
}
