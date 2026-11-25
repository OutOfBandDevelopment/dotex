using OoBDev.UiDefinitions;

namespace OoBDev.Midi.Panels;

/// <summary>The MIDI panel: sliders that set the channel, note, velocity and program the buttons use, buttons that send channel and real-time messages, and the last message received.</summary>
public static class MidiUiDefinition
{
    /// <summary>Builds the panel layout.</summary>
    public static UiDefinition Build() => new()
    {
        Name = "MIDI",
        Description = "A MIDI port on a serial line (31,250 baud, or the baud of a USB MIDI bridge).",
        Sections =
        [
            new UiSection
            {
                Label = "Message",
                Controls =
                [
                    new SliderControl { Id = "channel", Label = "Channel", Minimum = 1, Maximum = 16, DefaultValue = 1 },
                    new SliderControl { Id = "note", Label = "Note", Minimum = 0, Maximum = 127, DefaultValue = 60 },
                    new SliderControl { Id = "velocity", Label = "Velocity", Minimum = 0, Maximum = 127, DefaultValue = 100 },
                    new SliderControl { Id = "program", Label = "Program", Minimum = 0, Maximum = 127, DefaultValue = 0 },
                ],
            },
            new UiSection
            {
                Label = "Send",
                Controls =
                [
                    new ButtonControl { Id = "noteOn", Label = "Note on" },
                    new ButtonControl { Id = "noteOff", Label = "Note off" },
                    new ButtonControl { Id = "programChange", Label = "Program change" },
                    new ButtonControl { Id = "allNotesOff", Label = "All notes off" },
                    new ButtonControl { Id = "allSoundOff", Label = "All sound off" },
                ],
            },
            new UiSection
            {
                Label = "Real time",
                Controls =
                [
                    new ButtonControl { Id = "start", Label = "Start" },
                    new ButtonControl { Id = "continue", Label = "Continue" },
                    new ButtonControl { Id = "stop", Label = "Stop" },
                    new ButtonControl { Id = "reset", Label = "Reset" },
                ],
            },
            new UiSection
            {
                Label = "Received",
                Controls = [new IndicatorControl { Id = "last", Label = "Last message", DefaultValue = "-" }],
            },
        ],
    };
}
