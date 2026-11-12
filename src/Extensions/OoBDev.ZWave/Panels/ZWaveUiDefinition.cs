using System.Collections.Generic;
using OoBDev.UiDefinitions;

namespace OoBDev.ZWave.Panels;

/// <summary>The control layout of the Z-Wave controller panel: controller queries, single-node commands and network management.</summary>
public static class ZWaveUiDefinition
{
    /// <summary>The command ids the control surface accepts.</summary>
    public static IReadOnlyList<string> CommandIds { get; } =
    [
        "version", "memoryId", "initData", "softReset", "nodeInfo", "switchOn", "switchOff", "switchGet", "setLevel",
        "addNode", "stopAddNode", "removeNode", "stopRemoveNode",
    ];

    /// <summary>Builds the panel definition.</summary>
    public static UiDefinition Build() => new()
    {
        Name = "Z-Wave controller",
        Description = "Z-Wave USB stick over the Serial API (115,200 baud): controller queries, node switching and network management.",
        Sections =
        [
            new UiSection
            {
                Label = "Controller",
                Controls =
                [
                    new ButtonControl { Id = "version", Label = "Version" },
                    new ButtonControl { Id = "memoryId", Label = "Home and node id" },
                    new ButtonControl { Id = "initData", Label = "Node list" },
                    new ButtonControl { Id = "softReset", Label = "Soft reset", ConfirmMessage = "Restart the controller firmware? Pending network operations are lost." },
                ],
            },
            new UiSection
            {
                Label = "Node",
                Controls =
                [
                    new NumericControl { Id = "node", Label = "Node", Minimum = 1, Maximum = 232, DefaultValue = 2 },
                    new NumericControl { Id = "level", Label = "Level", Minimum = 0, Maximum = 99, DefaultValue = 50 },
                    new ButtonControl { Id = "nodeInfo", Label = "Node info", ParameterFieldIds = ["node"] },
                    new ButtonControl { Id = "switchOn", Label = "On", ParameterFieldIds = ["node"] },
                    new ButtonControl { Id = "switchOff", Label = "Off", ParameterFieldIds = ["node"] },
                    new ButtonControl { Id = "switchGet", Label = "Read switch", ParameterFieldIds = ["node"] },
                    new ButtonControl { Id = "setLevel", Label = "Set level", ParameterFieldIds = ["node", "level"] },
                ],
            },
            new UiSection
            {
                Label = "Network",
                Controls =
                [
                    new ButtonControl { Id = "addNode", Label = "Include node" },
                    new ButtonControl { Id = "stopAddNode", Label = "Stop including" },
                    new ButtonControl { Id = "removeNode", Label = "Exclude node" },
                    new ButtonControl { Id = "stopRemoveNode", Label = "Stop excluding" },
                ],
            },
            new UiSection
            {
                Label = "Status",
                Controls =
                [
                    new IndicatorControl { Id = "last", Label = "Last frame", DefaultValue = "-" },
                ],
            },
        ],
    };
}
