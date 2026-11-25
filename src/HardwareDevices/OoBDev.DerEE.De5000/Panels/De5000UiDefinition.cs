using OoBDev.UiDefinitions;

namespace OoBDev.DerEE.De5000.Panels;

/// <summary>The DE-5000 as a read-only panel: the meter transmits unprompted and takes no commands, so every control is an indicator for a value of <see cref="De5000Interpreter"/>.</summary>
public static class De5000UiDefinition
{
    /// <summary>Builds the panel layout.</summary>
    public static UiDefinition Build() => new()
    {
        Name = "DE-5000 LCR meter",
        Description = "Live readings of the DER EE DE-5000 (no controls: the meter only transmits).",
        Sections =
        [
            new UiSection
            {
                Label = "Measurement",
                Controls =
                [
                    new IndicatorControl { Id = "primary", Label = "Primary", DefaultValue = "-" },
                    new IndicatorControl { Id = "secondary", Label = "Secondary", DefaultValue = "-" },
                    new IndicatorControl { Id = "frequency", Label = "Frequency", DefaultValue = "-" },
                ],
            },
            new UiSection
            {
                Label = "Modes",
                Controls =
                [
                    Flag("hold", "Hold"),
                    Flag("delta", "Relative"),
                    Flag("referenceShown", "Reference shown"),
                    Flag("calibration", "Calibration"),
                    Flag("sorting", "Sorting"),
                    Flag("lcrAuto", "LCR auto"),
                    Flag("autoRange", "Auto range"),
                    Flag("parallel", "Parallel"),
                ],
            },
        ],
    };

    private static IndicatorControl Flag(string id, string label) => new() { Id = id, Label = label, DefaultValue = "0" };
}
