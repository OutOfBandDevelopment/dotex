using System.Collections.Generic;
using System.Xml.Serialization;

namespace OoBDev.UiDefinitions;

/// <summary>One labeled group of controls. See docs/design/ui-definitions.md.</summary>
public sealed class UiSection
{
    public string? Label { get; set; }

    /// <summary>When set, the whole section is shown only while this condition holds — honored by the form renderers (see docs/design/ui-definitions.md's "Forms from one definition").</summary>
    public UiCondition? VisibleWhen { get; set; }

    [XmlElement("Button", typeof(ButtonControl))]
    [XmlElement("Toggle", typeof(ToggleControl))]
    [XmlElement("Slider", typeof(SliderControl))]
    [XmlElement("Numeric", typeof(NumericControl))]
    [XmlElement("Choice", typeof(ChoiceControl))]
    [XmlElement("TextField", typeof(TextFieldControl))]
    [XmlElement("Indicator", typeof(IndicatorControl))]
    [XmlElement("BarGraph", typeof(BarGraphControl))]
    [XmlElement("StripChart", typeof(StripChartControl))]
    [XmlElement("Vector", typeof(VectorControl))]
    public List<UiControl> Controls { get; set; } = [];
}
