using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Examples.Controls.Blazor.Components;

namespace OoBDev.Examples.Controls.Blazor.Tests;

/// <summary>The sample page renders the generated form; the advanced section appears with its toggle.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ControlsPageTests
{
    [TestMethod]
    public void Render_ShowsTheSampleControls_AdvancedHiddenUntilToggled()
    {
        // Stage
        using var context = new Bunit.TestContext();

        // Test
        var cut = context.RenderComponent<ControlsPage>();

        // Assert
        Assert.AreEqual("sample", cut.Find("[data-id=Name] input[type=text]").GetAttribute("value"));
        Assert.AreEqual(3, cut.FindAll("[data-id=Mode] input[type=radio]").Count);
        Assert.AreEqual(0, cut.FindAll("[data-id=Notes]").Count);

        cut.Find("[data-id=ShowAdvanced] input[type=checkbox]").Change(true);
        Assert.AreEqual(1, cut.FindAll("[data-id=Notes]").Count);
    }
}
