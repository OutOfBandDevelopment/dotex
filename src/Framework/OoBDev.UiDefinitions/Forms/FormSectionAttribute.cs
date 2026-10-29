using System;

namespace OoBDev.UiDefinitions.Forms;

/// <summary>
/// Declares how one <c>[Category]</c> of a model's fields renders as a form section: its order, a
/// label different from the category name (an empty label draws no header), and a visibility
/// condition for the whole section (e.g. the Serial fields only while the transport is serial).
/// A category with no attribute gets a section labeled with the category name, in order of first
/// appearance, always shown.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class FormSectionAttribute : Attribute
{
    public FormSectionAttribute(string category)
    {
        Category = category;
    }

    public string Category { get; }

    /// <summary>The header text; the category name when null, no header when empty.</summary>
    public string? Label { get; set; }

    public int Order { get; set; }

    public string? VisibleWhen { get; set; }

    public string[]? VisibleWhenValues { get; set; }
}
