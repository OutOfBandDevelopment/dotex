using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.UiDefinitions;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class UiDefinitionEvaluatorTests
{
    private static readonly UiDefinition Bench = new UiDefinitionCatalog().Find("bench-supply")!;

    private static UiControlStateModel State(IReadOnlyList<UiControlStateModel> states, string id) => states.Single(s => s.Id == id);

    private static IReadOnlyList<UiControlStateModel> Evaluate(params (string Id, string Value)[] values) =>
        new UiDefinitionEvaluator().Evaluate(Bench, values.ToDictionary(v => v.Id, v => v.Value));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_NoValues_UsesDefaultsAndDisablesDependentControl()
    {
        var states = Evaluate();

        var voltage = State(states, "voltage");
        Assert.IsFalse(voltage.Enabled);
        Assert.AreEqual("Switch the output on first.", voltage.DisabledReason);
        Assert.AreEqual("5", voltage.Value);
        Assert.IsTrue(voltage.Valid);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_ToggleOn_EnablesDependentControl()
    {
        var voltage = State(Evaluate(("output", "true")), "voltage");

        Assert.IsTrue(voltage.Enabled);
        Assert.IsNull(voltage.DisabledReason);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_ModeConstantVoltage_HidesCurrentLimitSection()
    {
        var states = Evaluate();

        Assert.IsFalse(State(states, "current").Visible);
        Assert.IsFalse(State(states, "reset").Visible);
        Assert.IsTrue(State(states, "voltage").Visible);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_ModeConstantCurrent_ShowsCurrentLimitSection()
    {
        var states = Evaluate(("mode", "Constant current"));

        Assert.IsTrue(State(states, "current").Visible);
        Assert.IsTrue(State(states, "reset").Visible);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_VoltageAboveMaximum_IsInvalidWithRange()
    {
        var voltage = State(Evaluate(("voltage", "42")), "voltage");

        Assert.IsFalse(voltage.Valid);
        StringAssert.Contains(voltage.Error, "0 to 30");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_VoltageNotANumber_IsInvalid()
    {
        var voltage = State(Evaluate(("voltage", "abc")), "voltage");

        Assert.IsFalse(voltage.Valid);
        StringAssert.Contains(voltage.Error, "not a number");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_VoltageWithPadding_IsNormalized()
    {
        var voltage = State(Evaluate(("voltage", " 05 ")), "voltage");

        Assert.IsTrue(voltage.Valid);
        Assert.AreEqual("5", voltage.Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_NameTooLong_IsInvalid()
    {
        var name = State(Evaluate(("name", "0123456789abc")), "name");

        Assert.IsFalse(name.Valid);
        Assert.AreEqual("At most 12 characters.", name.Error);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_ModeNotAnOption_IsInvalid()
    {
        var mode = State(Evaluate(("mode", "Burst")), "mode");

        Assert.IsFalse(mode.Valid);
        Assert.AreEqual("Not one of the options.", mode.Error);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void DefaultValue_EachControlKind_ReturnsText()
    {
        Assert.AreEqual("false", UiDefinitionEvaluator.DefaultValue(new ToggleControl { Id = "a", Label = "a" }));
        Assert.AreEqual("Idle", UiDefinitionEvaluator.DefaultValue(new IndicatorControl { Id = "a", Label = "a", DefaultValue = "Idle" }));
        Assert.AreEqual("x", UiDefinitionEvaluator.DefaultValue(new ChoiceControl { Id = "a", Label = "a", Options = ["x", "y"] }));
        Assert.AreEqual(string.Empty, UiDefinitionEvaluator.DefaultValue(new ButtonControl { Id = "a", Label = "a" }));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Catalog_Generated_HasConditionalSectionFromTheModel()
    {
        var shipping = new UiDefinitionCatalog().Find("shipping")!;
        var states = new UiDefinitionEvaluator().Evaluate(shipping, new Dictionary<string, string> { ["Carrier"] = "Air" });

        Assert.AreEqual("Shipping preferences", shipping.Name);
        Assert.IsTrue(State(states, "WindowHours").Visible);
        Assert.IsFalse(State(new UiDefinitionEvaluator().Evaluate(shipping, new Dictionary<string, string>()), "WindowHours").Visible);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Catalog_List_ReturnsBothSamplesOrderedByKey()
    {
        var list = new UiDefinitionCatalog().List();

        CollectionAssert.AreEqual(new[] { "bench-supply", "shipping" }, list.Select(i => i.Key).ToArray());
        CollectionAssert.AreEqual(new[] { "hand-built", "generated" }, list.Select(i => i.Source).ToArray());
        Assert.IsNull(new UiDefinitionCatalog().Find("nope"));
    }
}
