using System.Collections.Generic;
using System.Linq;
using OoBDev.UiDefinitions;

namespace OoBDev.LanC.Panels;

/// <summary>The LANC remote as a panel: transport, zoom and focus buttons, each a two-byte command of the author's LANC to RS-485 bridge, and the last status the bridge reported.</summary>
public static class LanCUiDefinition
{
    /// <summary>The button command ids with the byte pair each sends.</summary>
    public static IReadOnlyList<(string Id, string Label, LanCSubCommand Table, byte Code)> Buttons { get; } =
    [
        ("play", "Play", LanCSubCommand.CameraVtrNormal, (byte)LanCCameraVtrCommand.Play),
        ("stop", "Stop", LanCSubCommand.CameraVtrNormal, (byte)LanCCameraVtrCommand.Stop),
        ("pause", "Pause", LanCSubCommand.CameraVtrNormal, (byte)LanCCameraVtrCommand.Pause),
        ("rewind", "Rewind", LanCSubCommand.CameraVtrNormal, (byte)LanCCameraVtrCommand.Rewind),
        ("fastForward", "Fast forward", LanCSubCommand.CameraVtrNormal, (byte)LanCCameraVtrCommand.FastForward),
        ("record", "Record", LanCSubCommand.CameraVtrNormal, (byte)LanCCameraVtrCommand.Record),
        ("eject", "Eject", LanCSubCommand.CameraVtrNormal, (byte)LanCCameraVtrCommand.Eject),
        ("zoomTele", "Zoom tele", LanCSubCommand.CameraSpecial, (byte)LanCCameraSpecialCommand.ZoomTelephoto3),
        ("zoomWide", "Zoom wide", LanCSubCommand.CameraSpecial, (byte)LanCCameraSpecialCommand.ZoomWide3),
        ("focusFar", "Focus far", LanCSubCommand.CameraSpecial, (byte)LanCCameraSpecialCommand.FocusFar),
        ("focusNear", "Focus near", LanCSubCommand.CameraSpecial, (byte)LanCCameraSpecialCommand.FocusNear),
        ("autoFocus", "Auto focus", LanCSubCommand.CameraSpecial, (byte)LanCCameraSpecialCommand.AutoFocusToggle),
        ("recordStart", "Camera record start", LanCSubCommand.CameraSpecial, (byte)LanCCameraSpecialCommand.RecordStart),
        ("recordStop", "Camera record stop", LanCSubCommand.CameraSpecial, (byte)LanCCameraSpecialCommand.RecordStop),
    ];

    /// <summary>Builds the panel layout.</summary>
    public static UiDefinition Build() => new()
    {
        Name = "LANC remote",
        Description = "Sony LANC camcorder or VTR through the LANC to RS-485 bridge (57,600 baud).",
        Sections =
        [
            Section("Transport", "play", "stop", "pause", "rewind", "fastForward", "record", "eject"),
            Section("Camera", "zoomTele", "zoomWide", "focusFar", "focusNear", "autoFocus", "recordStart", "recordStop"),
            new UiSection
            {
                Label = "Status",
                Controls =
                [
                    new IndicatorControl { Id = "status", Label = "Status", DefaultValue = "-" },
                    new IndicatorControl { Id = "frame", Label = "Last frame", DefaultValue = "-" },
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
