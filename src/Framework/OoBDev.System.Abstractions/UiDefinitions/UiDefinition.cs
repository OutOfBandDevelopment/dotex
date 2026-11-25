using System.Collections.Generic;

namespace OoBDev.UiDefinitions;

/// <summary>
/// A named, declarative description of a control panel — what controls exist and how they're
/// grouped, independent of any UI framework. See docs/design/UiDefinitions/ui-definitions.md.
/// </summary>
public sealed class UiDefinition
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public List<UiSection> Sections { get; set; } = [];
}
