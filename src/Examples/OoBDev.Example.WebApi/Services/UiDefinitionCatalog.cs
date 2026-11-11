using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.WebApi.Services;

/// <summary>The sample UI definitions the host serves: one built by hand, one generated from a model type.</summary>
public class UiDefinitionCatalog
{
    private const string BenchSupplyKey = "bench-supply";
    private const string ShippingKey = "shipping";

    private readonly Dictionary<string, (UiDefinition Definition, string Source)> _definitions = new(StringComparer.OrdinalIgnoreCase)
    {
        [BenchSupplyKey] = (BenchSupply(), "hand-built"),
        [ShippingKey] = (FormDefinitionGenerator.Generate<ShippingPreferencesForm>(), "generated"),
    };

    /// <summary>Lists the served definitions.</summary>
    /// <returns>One entry per definition, ordered by key.</returns>
    public virtual IReadOnlyList<UiDefinitionInfoModel> List() =>
    [
        .. _definitions.OrderBy(d => d.Key, StringComparer.Ordinal).Select(d => new UiDefinitionInfoModel
        {
            Key = d.Key,
            Name = d.Value.Definition.Name,
            Description = d.Value.Definition.Description,
            Source = d.Value.Source,
        }),
    ];

    /// <summary>Finds a definition by key.</summary>
    /// <param name="key">The key from <see cref="List"/>.</param>
    /// <returns>The definition, or null when the key is unknown.</returns>
    public virtual UiDefinition? Find(string key) => _definitions.TryGetValue(key, out var entry) ? entry.Definition : null;

    private static UiDefinition BenchSupply() => new()
    {
        Name = "Bench power supply",
        Description = "Written by hand: a toggle that enables a numeric field, a section shown for one mode, and a button.",
        Sections =
        [
            new UiSection
            {
                Label = "Output",
                Controls =
                [
                    new ToggleControl { Id = "output", Label = "Output enabled" },
                    new NumericControl
                    {
                        Id = "voltage", Label = "Voltage", Minimum = 0, Maximum = 30, DefaultValue = 5, Unit = "V",
                        EnabledWhen = new UiCondition { Id = "output" }, DisabledReason = "Switch the output on first.",
                    },
                    new ChoiceControl
                    {
                        Id = "mode", Label = "Mode", Options = ["Constant voltage", "Constant current"],
                        DefaultValue = "Constant voltage", Style = ChoiceStyle.RadioGroup,
                    },
                ],
            },
            new UiSection
            {
                Label = "Current limit",
                VisibleWhen = new UiCondition { Id = "mode", Values = ["Constant current"] },
                Controls =
                [
                    new SliderControl { Id = "current", Label = "Current", Minimum = 0, Maximum = 5, Step = 0.1, DefaultValue = 1, Unit = "A" },
                    new ButtonControl { Id = "reset", Label = "Reset limit", CommandId = "reset-limit", ConfirmMessage = "Reset the current limit?" },
                ],
            },
            new UiSection
            {
                Label = "Label",
                Controls =
                [
                    new TextFieldControl { Id = "name", Label = "Name", MaxLength = 12, DefaultValue = "Bench 1" },
                    new IndicatorControl { Id = "state", Label = "State", DefaultValue = "Idle" },
                ],
            },
        ],
    };
}
