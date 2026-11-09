using System;
using System.ComponentModel;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>One labeled group of panel controls.</summary>
[FormSection("Section", Order = 0, Label = "")]
[FormSection("Visibility", Order = 1)]
public sealed class SectionForm : EditorForm
{
    public SectionForm(UiSection section, Action edited)
        : base(edited)
    {
        Section = section;
    }

    public UiSection Section { get; }

    [Category("Section")]
    [DisplayName("Label")]
    [Description("The section header; blank for an always-open section with no header.")]
    [FormField(Order = 0)]
    public string? Label
    {
        get => Section.Label;
        set
        {
            Section.Label = NullIfBlank(value);
            Changed();
        }
    }

    public bool HasVisibleWhenId => Section.VisibleWhen is not null;

    [Category("Visibility")]
    [DisplayName("Visible when")]
    [Description("Another control's or value's id; the section is hidden unless its value matches (blank: always shown).")]
    [FormField(Order = 0)]
    public string? VisibleWhenId
    {
        get => Section.VisibleWhen?.Id;
        set
        {
            var id = NullIfBlank(value);
            Section.VisibleWhen = id is null ? null : new UiCondition { Id = id, Values = Section.VisibleWhen?.Values ?? [] };
            Changed(null);
        }
    }

    [Category("Visibility")]
    [DisplayName("Visible values")]
    [Description("Comma-separated values that satisfy \"Visible when\" (blank: the value must be true).")]
    [FormField(Order = 1, VisibleWhen = nameof(HasVisibleWhenId))]
    public string VisibleWhenValues
    {
        get => string.Join(", ", Section.VisibleWhen?.Values ?? []);
        set
        {
            if (Section.VisibleWhen is { } condition)
            {
                condition.Values = [.. FormBinding.SplitList(value)];
                Changed();
            }
        }
    }
}
