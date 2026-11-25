using System.Collections.Generic;
using System.Linq;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>A labelled group of rows.</summary>
/// <param name="section">The section definition.</param>
/// <param name="rows">The rows, in definition order.</param>
/// <param name="binding">The bound model that decides the section's own <see cref="UiSection.VisibleWhen"/>, or null for always shown.</param>
public sealed class FormSectionViewModel(UiSection section, IReadOnlyList<ControlViewModel> rows, FormBinding? binding = null) : ViewModelBase
{
    /// <summary>The section label, or null for an unlabelled section.</summary>
    public string? Label { get; } = section.Label;

    /// <summary>The rows in definition order.</summary>
    public IReadOnlyList<ControlViewModel> Rows { get; } = rows;

    /// <summary>False when the section's own condition does not hold or every row is hidden, so a view can drop the header too.</summary>
    public bool HasVisibleRows => (binding is null || binding.IsVisible(section.VisibleWhen)) && Rows.Any(r => r.IsVisible);

    /// <summary>Raises <see cref="ViewModelBase.PropertyChanged"/> for <see cref="HasVisibleRows"/> after the model or the rows changed, so a bound view shows or hides the section.</summary>
    public void Refresh() => OnPropertyChanged(nameof(HasVisibleRows));
}
