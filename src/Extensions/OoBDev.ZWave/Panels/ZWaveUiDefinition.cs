using System.Collections.Generic;
using OoBDev.UiDefinitions;

namespace OoBDev.ZWave.Panels;

/// <summary>The control layout of the Z-Wave controller panel: controller queries, single-node commands and network management.</summary>
public static class ZWaveUiDefinition
{
    /// <summary>The command ids the control surface accepts.</summary>
    public static IReadOnlyList<string> CommandIds { get; } =
    [
        "refresh", "version", "memoryId", "initData", "softReset", "nodeInfo", "switchOn", "switchOff", "switchGet", "setLevel",
        "addNode", "stopAddNode", "removeNode", "stopRemoveNode",
        "resetNetwork", "learnOn", "learnOff", "heal", "isFailed", "removeFailed", "configGet", "configSet", "sendRaw",
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
                    new ButtonControl { Id = "refresh", Label = "Refresh" },
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
                    new TextFieldControl { Id = "config", Label = "Configuration", DefaultValue = "1" },
                    new ButtonControl { Id = "configGet", Label = "Configuration get", ParameterFieldIds = ["node", "config"] },
                    new ButtonControl { Id = "configSet", Label = "Configuration set", ParameterFieldIds = ["node", "config"], ConfirmMessage = "Write this configuration value to the device? It changes the device's stored configuration." },
                    new TextFieldControl { Id = "payload", Label = "Command class payload (hex)", DefaultValue = "26 01 32" },
                    new ButtonControl { Id = "sendRaw", Label = "Send to node", ParameterFieldIds = ["node", "payload"] },
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
                    new ButtonControl { Id = "learnOn", Label = "Join another network", ConfirmMessage = "Make the stick a secondary controller of the network that includes it?" },
                    new ButtonControl { Id = "learnOff", Label = "Stop learn mode" },
                    new ButtonControl { Id = "heal", Label = "Heal node", ParameterFieldIds = ["node"] },
                    new ButtonControl { Id = "isFailed", Label = "Is node failed?", ParameterFieldIds = ["node"] },
                    new ButtonControl { Id = "removeFailed", Label = "Remove failed node", ParameterFieldIds = ["node"], ConfirmMessage = "Remove this node from the network? The controller only removes a node it marks as failed." },
                    new ButtonControl { Id = "resetNetwork", Label = "Reset network", ConfirmMessage = "Erase every pairing and create a new home id? This cannot be undone." },
                ],
            },
            new UiSection
            {
                Label = "Status",
                Controls =
                [
                    new IndicatorControl { Id = "last", Label = "Last frame", DefaultValue = "-" },
                    new IndicatorControl { Id = "library", Label = "Library", DefaultValue = "-" },
                    new IndicatorControl { Id = "serialApi", Label = "Serial API", DefaultValue = "-" },
                    new IndicatorControl { Id = "manufacturer", Label = "Manufacturer", DefaultValue = "-" },
                    new IndicatorControl { Id = "product", Label = "Product", DefaultValue = "-" },
                    new IndicatorControl { Id = "functions", Label = "Functions", DefaultValue = "-" },
                    new IndicatorControl { Id = "homeId", Label = "Home ID", DefaultValue = "-" },
                    new IndicatorControl { Id = "controllerNode", Label = "Controller node", DefaultValue = "-" },
                    new IndicatorControl { Id = "role", Label = "Role", DefaultValue = "-" },
                    new IndicatorControl { Id = "chip", Label = "Chip", DefaultValue = "-" },
                    new IndicatorControl { Id = "sucNode", Label = "SUC node", DefaultValue = "-" },
                    new IndicatorControl { Id = "nodeCount", Label = "Node count", DefaultValue = "-" },
                    new IndicatorControl { Id = "nodes", Label = "Nodes", DefaultValue = "-" },
                ],
            },
        ],
    };
}
