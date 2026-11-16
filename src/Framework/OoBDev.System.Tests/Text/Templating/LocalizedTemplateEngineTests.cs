using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using System.Globalization;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.Text.Templating;

[TestClass, DoNotParallelize]
public class LocalizedTemplateEngineTests
{
    private static Mock<ITemplateEngine> Engine(params string[] registered)
    {
        var engine = new Mock<ITemplateEngine>();
        foreach (var name in registered)
        {
            engine.Setup(e => e.Get(name)).Returns(new Mock<ITemplateContext>().Object);
            engine.Setup(e => e.ApplyAsync(name, It.IsAny<object>())).ReturnsAsync($"rendered {name}");
        }

        return engine;
    }

    private static async Task<string?> RenderAs(string culture, Mock<ITemplateEngine> engine)
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(culture);
        try
        {
            return await new LocalizedTemplateEngine(engine.Object).ApplyAsync("welcome", new { });
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ApplyAsync_ExactCulture_Wins() =>
        Assert.AreEqual("rendered welcome.fr-CA", await RenderAs("fr-CA", Engine("welcome", "welcome.fr", "welcome.fr-CA")));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ApplyAsync_FallsBackToTheParentCulture() =>
        Assert.AreEqual("rendered welcome.fr", await RenderAs("fr-CA", Engine("welcome", "welcome.fr")));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ApplyAsync_NoTranslation_UsesThePlainName() =>
        Assert.AreEqual("rendered welcome", await RenderAs("de-DE", Engine("welcome", "welcome.fr")));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Get_ResolvesTheTranslatedName()
    {
        var engine = Engine("welcome", "welcome.es");
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("es-MX");
        try
        {
            new LocalizedTemplateEngine(engine.Object).Get("welcome");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }

        engine.Verify(e => e.Get("welcome.es"), Times.AtLeastOnce);
    }
}
