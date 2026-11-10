using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System.IO;
using System.Linq;

namespace OoBDev.Scpi.Tests;

/// <summary>A profile that names another in <c>Extends</c> is layered over it: base commands first, own commands replace by id.</summary>
[TestClass]
[TestCategory(TestCategories.Scpi)]
[TestCategory(TestCategories.Unit)]
public sealed class ScpiProfileLayeringTests
{
    private const string _family = """
        { "Name": "Acme family", "Terminator": "\r\n", "Notes": "family notes",
          "Commands": [ { "Id": "idn", "Label": "Identify", "Template": "*IDN?", "IsQuery": true },
                        { "Id": "volt", "Label": "Voltage", "Template": "VOLT?", "IsQuery": true } ] }
        """;

    private static ScpiProfileCatalog Load(params (string Name, string Json)[] files)
    {
        using var folder = new TempDirectoryHandle();
        foreach (var (name, json) in files)
        {
            File.WriteAllText(Path.Combine(folder.DirectoryPath, name), json);
        }

        return ScpiProfileCatalog.Create([folder.DirectoryPath]);
    }

    private static ScpiInstrumentProfile Find(ScpiProfileCatalog catalog, string name) => catalog.Profiles.Single(p => p.Name == name);

    [TestMethod]
    public void Extends_AFamilyProfile_KeepsBaseCommandsFirst_ReplacesBySameId_AndInheritsWhatItOmits()
    {
        var catalog = Load(
            ("a-family.json", _family),
            ("b-model.json", """
                { "Name": "Acme X1", "IdnPattern": "ACME,X1", "Extends": "Acme family",
                  "Commands": [ { "Id": "volt", "Label": "Voltage (X1)", "Template": "MEAS:VOLT?", "IsQuery": true },
                                { "Id": "temp", "Label": "Temperature", "Template": "TEMP?", "IsQuery": true } ] }
                """));

        var model = Find(catalog, "Acme X1");

        CollectionAssert.AreEqual(new[] { "idn", "volt", "temp" }, model.Commands.Select(c => c.Id).ToArray());
        Assert.AreEqual("MEAS:VOLT?", model.Commands.Single(c => c.Id == "volt").Template);
        Assert.AreEqual("\r\n", model.Terminator, "The terminator is inherited when the file leaves it out.");
        Assert.AreEqual("family notes", model.Notes);
        Assert.IsEmpty(catalog.LoadErrors);
        Assert.IsNull(Find(catalog, "Acme family").IdnPattern, "The pattern is never inherited, so the family is not auto-selected.");
    }

    [TestMethod]
    public void Extends_AnOwnTerminator_Wins()
    {
        var catalog = Load(
            ("a-family.json", _family),
            ("b-model.json", """{ "Name": "Acme X2", "Extends": "Acme family", "Terminator": "\n", "Commands": [] }"""));

        Assert.AreEqual("\n", Find(catalog, "Acme X2").Terminator);
    }

    [TestMethod]
    public void Extends_TheGenericProfile_AddsTheCommonCommands()
    {
        var catalog = Load(("a.json", """{ "Name": "Plain", "Extends": "Generic (manual)", "Commands": [ { "Id": "go", "Label": "Go", "Template": "GO" } ] }"""));

        var ids = Find(catalog, "Plain").Commands.Select(c => c.Id).ToArray();

        CollectionAssert.AreEqual(new[] { "idn", "rst", "cls", "opc", "go" }, ids);
    }

    [TestMethod]
    public void Extends_ChainsThroughSeveralLayers_EvenWhenTheBaseLoadsLater()
    {
        var catalog = Load(
            ("a-leaf.json", """{ "Name": "Leaf", "Extends": "Middle", "Commands": [ { "Id": "leaf", "Label": "L", "Template": "L" } ] }"""),
            ("b-middle.json", """{ "Name": "Middle", "Extends": "Acme family", "Commands": [ { "Id": "mid", "Label": "M", "Template": "M" } ] }"""),
            ("c-family.json", _family));

        CollectionAssert.AreEqual(new[] { "idn", "volt", "mid", "leaf" }, Find(catalog, "Leaf").Commands.Select(c => c.Id).ToArray());
    }

    [TestMethod]
    public void Extends_AMissingBase_IsReportedAndTheProfileDropped()
    {
        var catalog = Load(("a.json", """{ "Name": "Orphan", "Extends": "Nothing", "Commands": [] }"""));

        Assert.IsFalse(catalog.Profiles.Any(p => p.Name == "Orphan"));
        Assert.IsTrue(catalog.LoadErrors.Any(e => e.StartsWith("a.json", global::System.StringComparison.Ordinal) && e.Contains("'Nothing'", global::System.StringComparison.Ordinal)));
    }

    [TestMethod]
    public void Extends_ACycle_IsReportedAndBothProfilesDropped()
    {
        var catalog = Load(
            ("a.json", """{ "Name": "Ping", "Extends": "Pong", "Commands": [] }"""),
            ("b.json", """{ "Name": "Pong", "Extends": "Ping", "Commands": [] }"""));

        Assert.IsFalse(catalog.Profiles.Any(p => p.Name is "Ping" or "Pong"));
        Assert.IsNotEmpty(catalog.LoadErrors);
    }
}
