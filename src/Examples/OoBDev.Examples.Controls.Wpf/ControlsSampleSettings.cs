using System.Collections.Generic;
using System.ComponentModel;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.Examples.Controls.Wpf;

/// <summary>One property per control kind the form generator supports; the sample form is generated from this class.</summary>
[FormSection("Basics", Order = 0)]
[FormSection("Ranges", Order = 1)]
[FormSection("Advanced", Order = 2, VisibleWhen = nameof(ShowAdvanced))]
public sealed class ControlsSampleSettings
{
    public static IReadOnlyList<string> Modes { get; } = ["Low", "Medium", "High"];

    [Category("Basics")]
    [DisplayName("Enabled")]
    [FormField(Kind = FormFieldKind.Toggle, Order = 0)]
    public bool Enabled { get; set; } = true;

    [Category("Basics")]
    [DisplayName("Name")]
    [FormField(Kind = FormFieldKind.TextField, Order = 1, MaxLength = 20)]
    public string Name { get; set; } = "sample";

    [Category("Basics")]
    [DisplayName("Mode")]
    [FormField(Kind = FormFieldKind.Choice, OptionsFrom = nameof(Modes), ChoiceStyle = ChoiceStyle.RadioGroup, Order = 2)]
    public string Mode { get; set; } = "Medium";

    [Category("Basics")]
    [DisplayName("Show advanced")]
    [FormField(Kind = FormFieldKind.Toggle, Order = 3)]
    public bool ShowAdvanced { get; set; }

    [Category("Ranges")]
    [DisplayName("Count")]
    [FormField(Kind = FormFieldKind.Numeric, Minimum = 0, Maximum = 100, Unit = "items", Order = 0)]
    public int Count { get; set; } = 10;

    [Category("Ranges")]
    [DisplayName("Level")]
    [FormField(Kind = FormFieldKind.Slider, Minimum = 0, Maximum = 10, Order = 1)]
    public double Level { get; set; } = 5;

    [Category("Advanced")]
    [DisplayName("Notes")]
    [FormField(Kind = FormFieldKind.TextField, MaxLength = 200)]
    public string Notes { get; set; } = string.Empty;
}
