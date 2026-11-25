using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Examples.Controls.Blazor.Components;

namespace OoBDev.Examples.Controls.Blazor.Tests;

/// <summary>Each control on the page changes the preview and the bound-model table, so a control that does nothing is caught.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ControlsPageEffectTests
{
    [TestMethod]
    public void Render_Defaults_PreviewAndReadoutShowTheSettings()
    {
        // Stage
        using var context = new Bunit.TestContext();

        // Test
        var cut = context.RenderComponent<ControlsPage>();

        // Assert
        Assert.AreEqual("sample", cut.Find("[data-preview=name]").TextContent);
        Assert.AreEqual("Medium", cut.Find("[data-preview=mode]").TextContent);
        Assert.AreEqual("sample", cut.Find("[data-readout=Name] td").TextContent);
        Assert.AreEqual("true", cut.Find("[data-readout=Enabled] td").TextContent);
        Assert.AreEqual("false", cut.Find("[data-readout=ShowAdvanced] td").TextContent);
    }

    [TestMethod]
    public void Change_EachControl_UpdatesThePreviewAndTheReadout()
    {
        // Stage
        using var context = new Bunit.TestContext();
        var cut = context.RenderComponent<ControlsPage>();

        // Test
        cut.Find("[data-id=Name] input[type=text]").Change("probe");
        global::System.Linq.Enumerable.ToArray(cut.FindAll("[data-id=Mode] input[type=radio]"))[2].Change("High");
        cut.Find("[data-id=Enabled] input[type=checkbox]").Change(false);

        // Assert
        Assert.AreEqual("probe", cut.Find("[data-preview=name]").TextContent);
        Assert.AreEqual("probe", cut.Find("[data-readout=Name] td").TextContent);
        Assert.AreEqual("High", cut.Find("[data-readout=Mode] td").TextContent);
        Assert.IsTrue(cut.Find("[data-preview=mode]").ClassList.Contains("mode-high"));
        Assert.AreEqual("disabled", cut.Find("[data-preview=state]").TextContent);
        Assert.IsTrue(cut.Find("[data-id=preview]").ClassList.Contains("off"));
    }

    [TestMethod]
    public void Change_ShowAdvanced_RevealsTheNotesRowAndThePreviewNotes()
    {
        // Stage
        using var context = new Bunit.TestContext();
        var cut = context.RenderComponent<ControlsPage>();
        Assert.AreEqual(0, cut.FindAll("[data-preview=notes]").Count);

        // Test
        cut.Find("[data-id=ShowAdvanced] input[type=checkbox]").Change(true);
        cut.Find("[data-id=Notes] input[type=text]").Change("bench 2");

        // Assert
        Assert.AreEqual("bench 2", cut.Find("[data-preview=notes]").TextContent);
        Assert.AreEqual("bench 2", cut.Find("[data-readout=Notes] td").TextContent);
    }
}
