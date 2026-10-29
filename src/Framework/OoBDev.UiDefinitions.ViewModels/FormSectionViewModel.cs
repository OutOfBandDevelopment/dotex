using System.Collections.Generic;
using System.Linq;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>A labelled group of rows.</summary>
/// <param name="section">The section definition.</param>
/// <param name="rows">The rows, in definition order.</param>
public sealed class FormSectionViewModel(UiSection section, IReadOnlyList<ControlViewModel> rows)
{
    /// <summary>The section label, or null for an unlabelled section.</summary>
    public string? Label { get; } = section.Label;

    /// <summary>The rows in definition order.</summary>
    public IReadOnlyList<ControlViewModel> Rows { get; } = rows;

    /// <summary>False when every row is hidden, so a view can drop the header too.</summary>
    public bool HasVisibleRows => Rows.Any(r => r.IsVisible);
}
