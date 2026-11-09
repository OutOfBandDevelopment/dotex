using OoBDev.UiDefinitions;

namespace OoBDev.Fencing.ScoreMachines.Panels;

/// <summary>The score box as a read-only panel: the machine transmits unprompted and takes no commands, so every control is an indicator for a value published by <see cref="ScoreMachineDecoder"/>.</summary>
public static class FencingUiDefinition
{
    /// <summary>Builds the panel layout for a machine.</summary>
    /// <param name="name">The machine name shown as the panel title.</param>
    public static UiDefinition Build(string name) => new()
    {
        Name = name,
        Description = $"Live state of the {name} score machine (no controls: the machine only transmits).",
        Sections =
        [
            new UiSection
            {
                Label = "Bout",
                Controls =
                [
                    new IndicatorControl { Id = "clock", Label = "Clock", DefaultValue = "-" },
                    new IndicatorControl { Id = "match", Label = "Match", DefaultValue = "-" },
                ],
            },
            Side("left", "Left (red)"),
            Side("right", "Right (green)"),
        ],
    };

    private static UiSection Side(string prefix, string label) => new()
    {
        Label = label,
        Controls =
        [
            new IndicatorControl { Id = $"{prefix}Score", Label = "Score", DefaultValue = "-" },
            new IndicatorControl { Id = $"{prefix}Lights", Label = "Lights", DefaultValue = "-" },
            new IndicatorControl { Id = $"{prefix}Cards", Label = "Cards", DefaultValue = "-" },
            new IndicatorControl { Id = $"{prefix}Priority", Label = "Priority", DefaultValue = "-" },
        ],
    };
}
