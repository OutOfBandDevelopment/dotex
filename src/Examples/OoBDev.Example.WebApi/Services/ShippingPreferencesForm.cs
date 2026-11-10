using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;
using System.Collections.Generic;
using System.ComponentModel;

namespace OoBDev.WebApi.Services;

/// <summary>A plain model whose attributes become a UI definition through <see cref="FormDefinitionGenerator"/>.</summary>
[DisplayName("Shipping preferences")]
[Description("Generated from a C# model with FormField attributes; no form code was written.")]
[FormSection("Delivery", Order = 0, Label = "Delivery")]
[FormSection("Express", Order = 1, Label = "Express options", VisibleWhen = nameof(Carrier), VisibleWhenValues = ["Air"])]
public sealed class ShippingPreferencesForm
{
    /// <summary>The carriers offered.</summary>
    public IReadOnlyList<string> Carriers { get; } = ["Ground", "Air"];

    /// <summary>The chosen carrier.</summary>
    [Category("Delivery")]
    [FormField(Order = 0, OptionsFrom = nameof(Carriers), ChoiceStyle = ChoiceStyle.RadioGroup)]
    public string Carrier { get; set; } = "Ground";

    /// <summary>The number of parcels.</summary>
    [Category("Delivery")]
    [FormField(Order = 1, ValueKind = ValueKind.Integer, Minimum = 1, Maximum = 10)]
    public string Parcels { get; set; } = "1";

    /// <summary>Whether a signature is required.</summary>
    [Category("Delivery")]
    [FormField(Order = 2)]
    public bool SignatureRequired { get; set; }

    /// <summary>The delivery window in hours.</summary>
    [Category("Express")]
    [FormField(Kind = FormFieldKind.Slider, Minimum = 1, Maximum = 24, Unit = "h")]
    public double WindowHours { get; set; } = 4;
}
