using OoBDev.UiDefinitions;

namespace OoBDev.Quarta.RadexOne.Panels;

/// <summary>The Radex One panel: query buttons, the accumulated-dose reset, and the last values the device reported. The readings carry no unit because it is unconfirmed (see <see cref="RadexOneReadData"/>).</summary>
public static class RadexOneUiDefinition
{
    /// <summary>Read Data command id.</summary>
    public const string ReadData = "readData";

    /// <summary>Read Settings command id.</summary>
    public const string ReadSettings = "readSettings";

    /// <summary>Read Serial Version command id.</summary>
    public const string ReadSerialVersion = "readSerialVersion";

    /// <summary>Reset Accumulated command id.</summary>
    public const string ResetAccumulated = "resetAccumulated";

    /// <summary>Builds the panel layout.</summary>
    public static UiDefinition Build() => new()
    {
        Name = "Radex One",
        Description = "Quarta Radex One dosimeter: query the readings and settings, or reset the accumulated dose.",
        Sections =
        [
            new UiSection
            {
                Label = "Query",
                Controls =
                [
                    new ButtonControl { Id = ReadData, Label = "Read data" },
                    new ButtonControl { Id = ReadSettings, Label = "Read settings" },
                    new ButtonControl { Id = ReadSerialVersion, Label = "Read serial and version" },
                ],
            },
            new UiSection
            {
                Label = "Readings",
                Controls =
                [
                    new IndicatorControl { Id = "ambient", Label = "Ambient", DefaultValue = "-" },
                    new IndicatorControl { Id = "accumulated", Label = "Accumulated", DefaultValue = "-" },
                    new IndicatorControl { Id = "cpm", Label = "Counts per minute", DefaultValue = "-" },
                ],
            },
            new UiSection
            {
                Label = "Settings",
                Controls =
                [
                    new IndicatorControl { Id = "alarmMode", Label = "Alarm", DefaultValue = "-" },
                    new IndicatorControl { Id = "threshold", Label = "Threshold", DefaultValue = "-" },
                    new IndicatorControl { Id = "serialVersion", Label = "Serial and version", DefaultValue = "-" },
                ],
            },
            new UiSection
            {
                Label = "Maintenance",
                Controls = [new ButtonControl { Id = ResetAccumulated, Label = "Reset accumulated dose" }],
            },
        ],
    };
}
