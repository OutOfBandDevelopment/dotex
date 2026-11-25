using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>The definition's description is the form's Notes.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class FormNotesTests
{
    private sealed class Model
    {
        public string Name { get; set; } = string.Empty;
    }

    private static FormViewModel Build(string? description) =>
        new(
            new UiDefinition { Name = "n", Description = description, Sections = [new UiSection { Label = "S", Controls = [new TextFieldControl { Id = "Name", Label = "Name" }] }] },
            new FormBinding(new Model()));

    [TestMethod]
    public void Notes_DefinitionWithDescription_ReturnsItAndHasNotes()
    {
        // Stage
        using var form = Build("Wire the probe first.");

        // Assert
        Assert.AreEqual("Wire the probe first.", form.Notes);
        Assert.IsTrue(form.HasNotes);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("  ")]
    public void Notes_BlankDescription_IsNullAndHasNoNotes(string? description)
    {
        // Stage
        using var form = Build(description);

        // Assert
        Assert.IsNull(form.Notes);
        Assert.IsFalse(form.HasNotes);
    }
}
