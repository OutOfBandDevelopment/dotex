using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>One editable setting of a device configuration editor.</summary>
/// <param name="id">The control id the editor keys the value by.</param>
/// <param name="label">The text shown beside the field.</param>
public abstract class ConfigFieldViewModel(string id, string label) : ViewModelBase
{
    /// <summary>The control id.</summary>
    public string Id { get; } = id;

    /// <summary>The label.</summary>
    public string Label { get; } = label;
}
