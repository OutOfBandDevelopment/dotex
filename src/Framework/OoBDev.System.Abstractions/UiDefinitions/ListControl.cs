using System.Collections.Generic;

namespace OoBDev.UiDefinitions;

/// <summary>
/// A list of items with one selected (saved profiles, for example), the master half of a master-detail view. <see cref="UiControl.Id"/> names the model property
/// that holds the selected item; the detail is the form generated for that item, and the host places it beside the list.
/// </summary>
public sealed class ListControl : UiControl
{
    /// <summary>The model property that supplies the items; a form view model re-reads it on every refresh so added and deleted items show.</summary>
    public string? ItemsSource { get; set; }

    /// <summary>The items when the list is fixed (no <see cref="ItemsSource"/>).</summary>
    public List<string> Items { get; set; } = [];

    /// <summary>The id of the button control that adds an item, or null when the list cannot grow.</summary>
    public string? AddCommand { get; set; }

    /// <summary>The id of the button control that deletes the selected item, or null when items cannot be removed.</summary>
    public string? DeleteCommand { get; set; }

    /// <summary>The text of the item selected at the start.</summary>
    public string? DefaultValue { get; set; }
}
