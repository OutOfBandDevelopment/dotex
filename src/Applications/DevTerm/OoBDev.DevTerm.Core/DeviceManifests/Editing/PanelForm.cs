using System;
using System.ComponentModel;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>The panel's own name and notes.</summary>
[FormSection("Panel", Order = 0, Label = "")]
public sealed class PanelForm : EditorForm
{
    public PanelForm(UiDefinition ui, Action edited)
        : base(edited)
    {
        Ui = ui;
    }

    public UiDefinition Ui { get; }

    [Category("Panel")]
    [DisplayName("Panel name")]
    [FormField(Order = 0)]
    public string Name
    {
        get => Ui.Name;
        set
        {
            Ui.Name = value;
            Changed();
        }
    }

    [Category("Panel")]
    [DisplayName("Notes")]
    [Description("Shown in the panel's collapsible Notes section; the manifest's description when blank.")]
    [FormField(Order = 1)]
    public string? Notes
    {
        get => Ui.Description;
        set
        {
            Ui.Description = NullIfBlank(value);
            Changed();
        }
    }
}
