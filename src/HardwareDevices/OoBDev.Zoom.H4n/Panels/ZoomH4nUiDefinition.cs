using System.Collections.Generic;
using System.Linq;
using OoBDev.UiDefinitions;

namespace OoBDev.Zoom.H4n.Panels;

/// <summary>The Zoom H4n remote as a panel: the twelve remote buttons in transport, level and input groups, and the recorder's five documented status bits as indicators.</summary>
public static class ZoomH4nUiDefinition
{
    /// <summary>The button command ids in the order of <see cref="ZoomH4nButton"/>.</summary>
    public static IReadOnlyList<(string Id, string Label, ZoomH4nButton Button)> Buttons { get; } =
    [
        ("record", "Record", ZoomH4nButton.Record),
        ("play", "Play", ZoomH4nButton.Play),
        ("stop", "Stop", ZoomH4nButton.Stop),
        ("fastForward", "Fast forward", ZoomH4nButton.FastForward),
        ("rewind", "Rewind", ZoomH4nButton.Rewind),
        ("volumeUp", "Volume +", ZoomH4nButton.VolumeUp),
        ("volumeDown", "Volume -", ZoomH4nButton.VolumeDown),
        ("recordLevelUp", "Rec level +", ZoomH4nButton.RecordLevelUp),
        ("recordLevelDown", "Rec level -", ZoomH4nButton.RecordLevelDown),
        ("mic", "Mic", ZoomH4nButton.Mic),
        ("channel1", "Channel 1", ZoomH4nButton.Channel1),
        ("channel2", "Channel 2", ZoomH4nButton.Channel2),
    ];

    /// <summary>The status indicator ids, each shown as <c>On</c> or <c>Off</c>.</summary>
    public static IReadOnlyList<string> StatusIds { get; } = ["statusRecord", "statusPeak", "statusMic", "statusLed1", "statusLed2"];

    /// <summary>Builds the panel layout.</summary>
    public static UiDefinition Build() => new()
    {
        Name = "Zoom H4n",
        Description = "RC04 / RC2 remote port: press the recorder's buttons and watch its status LEDs.",
        Sections =
        [
            Section("Transport", "record", "play", "stop", "fastForward", "rewind"),
            Section("Levels", "volumeUp", "volumeDown", "recordLevelUp", "recordLevelDown"),
            Section("Input", "mic", "channel1", "channel2"),
            new UiSection
            {
                Label = "Status",
                Controls =
                [
                    new IndicatorControl { Id = "statusRecord", Label = "Record", DefaultValue = "Off" },
                    new IndicatorControl { Id = "statusPeak", Label = "Peak", DefaultValue = "Off" },
                    new IndicatorControl { Id = "statusMic", Label = "Mic", DefaultValue = "Off" },
                    new IndicatorControl { Id = "statusLed1", Label = "LED 1", DefaultValue = "Off" },
                    new IndicatorControl { Id = "statusLed2", Label = "LED 2", DefaultValue = "Off" },
                ],
            },
        ],
    };

    private static UiSection Section(string label, params string[] ids) => new()
    {
        Label = label,
        Controls = [.. ids.Select(id => Buttons.First(b => b.Id == id)).Select(b => (UiControl)new ButtonControl { Id = b.Id, Label = b.Label })],
    };
}
