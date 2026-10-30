using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions;

namespace OoBDev.Scpi.Panels.Tests;

/// <summary>A panel generated from a profile, driven through its binding like a renderer does.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ScpiPanelBuilderTests
{
    private static ScpiInstrumentProfile Profile() => new()
    {
        Name = "Test PSU",
        Terminator = "\n",
        Commands =
        [
            new ScpiCommandDefinition { Id = "idn", Label = "Identify", Category = "System", Template = "*IDN?", IsQuery = true },
            new ScpiCommandDefinition
            {
                Id = "set-volt",
                Label = "Set voltage",
                Category = "Output",
                Template = "VOLT {Voltage}",
                Parameters = [new ScpiParameterDefinition { Name = "Voltage", Kind = ScpiParameterKind.Numeric, Minimum = 0, Maximum = 30, Unit = "V", DefaultValue = "5" }],
            },
            new ScpiCommandDefinition
            {
                Id = "range",
                Label = "Range",
                Category = "Output",
                Template = "RANG {Range} {Note}",
                Parameters =
                [
                    new ScpiParameterDefinition { Name = "Range", Kind = ScpiParameterKind.Choice, Options = ["LOW", "HIGH"] },
                    new ScpiParameterDefinition { Name = "Note", Kind = ScpiParameterKind.Text, DefaultValue = "x" },
                ],
            },
        ],
    };

    [TestMethod]
    public void Build_Profile_MakesSectionPerCategoryWithControls()
    {
        // Test
        var panel = ScpiPanelBuilder.Build(Profile(), _ => Task.FromResult<string?>(null));

        // Assert
        Assert.AreEqual("Test PSU", panel.Definition.Name);
        CollectionAssert.AreEqual(new[] { "System", "Output" }, panel.Definition.Sections.Select(s => s.Label).ToArray());
        var output = panel.Definition.Sections[1].Controls;
        CollectionAssert.AreEqual(new[] { "set_volt_Voltage", "set_volt", "range_Range", "range_Note", "range" }, output.Select(c => c.Id).ToArray());
        Assert.IsInstanceOfType<NumericControl>(output[0]);
        Assert.AreEqual("Voltage (V)", output[0].Label);
        Assert.IsInstanceOfType<ChoiceControl>(output[2]);
        CollectionAssert.AreEqual(new[] { "set_volt_Voltage" }, ((ButtonControl)output[1]).ParameterFieldIds!);
        Assert.IsTrue(panel.Definition.Sections[0].Controls.Any(c => c.Id == "idn_reply"));
    }

    [TestMethod]
    public void Invoke_ButtonWithFieldValues_SendsBuiltRequest()
    {
        // Stage
        var sent = new List<ScpiRequest>();
        var panel = ScpiPanelBuilder.Build(Profile(), r =>
        {
            sent.Add(r);
            return Task.FromResult<string?>(null);
        });

        // Test
        panel.Binding.SetText(new TextFieldControl { Id = "set_volt_Voltage", Label = "V", Constraint = new ValueConstraint { Kind = ValueKind.Number } }, "12.5");
        var button = (ButtonControl)panel.Definition.Sections[1].Controls[1];
        var parameter = string.Join(',', button.ParameterFieldIds!.Select(panel.Binding.GetText));
        panel.Binding.Invoke(button.Id, parameter);

        // Assert
        Assert.AreEqual("VOLT 12.5\n", sent.Single().WireText);
    }

    [TestMethod]
    public async Task ExecuteAsync_Query_ShowsReplyThroughBinding()
    {
        // Stage
        var panel = ScpiPanelBuilder.Build(Profile(), _ => Task.FromResult<string?>("ACME,PSU,1,2"));
        var changed = new List<string?>();
        panel.Binding.Changed += (_, id) => changed.Add(id);

        // Test
        await ((ScpiPanelCommand)panel.Binding.GetValue("idn")!).ExecuteAsync(null).ConfigureAwait(false);

        // Assert
        Assert.AreEqual("ACME,PSU,1,2", panel.Binding.GetText("idn_reply"));
        CollectionAssert.Contains(changed, "idn_reply");
    }

    [TestMethod]
    public async Task ExecuteAsync_SendFails_ShowsErrorInReply()
    {
        // Stage
        var panel = ScpiPanelBuilder.Build(Profile(), _ => throw new global::System.InvalidOperationException("port closed"));

        // Test
        await ((ScpiPanelCommand)panel.Binding.GetValue("idn")!).ExecuteAsync(null).ConfigureAwait(false);

        // Assert
        Assert.AreEqual("Error: port closed", panel.Binding.GetText("idn_reply"));
    }

    [TestMethod]
    public void Build_ChoiceAndTextDefaults_SeedModel()
    {
        // Test
        var panel = ScpiPanelBuilder.Build(Profile(), _ => Task.FromResult<string?>(null));

        // Assert
        Assert.AreEqual("LOW", panel.Binding.GetText("range_Range"));
        Assert.AreEqual("x", panel.Binding.GetText("range_Note"));
        Assert.AreEqual("5", panel.Binding.GetText("set_volt_Voltage"));
    }

    [TestMethod]
    public void Build_NullProfile_Throws()
    {
        // Assert
        Assert.ThrowsExactly<global::System.ArgumentNullException>(() => ScpiPanelBuilder.Build(null!, _ => Task.FromResult<string?>(null)));
    }
}
