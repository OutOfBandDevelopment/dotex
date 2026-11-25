using OoBDev.Handlebars.Templating;
using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Handlebars.Tests.Templating;

[TestClass]
public class HandlebarsEscapingTests
{
    private static async Task<string> RenderAsync(HandlebarsTemplateProvider provider, string template, object data)
    {
        var source = new MemoryStream(Encoding.UTF8.GetBytes(template));
        var context = new TemplateContext
        {
            OpenTemplate = _ => source,
            TemplateName = "escape",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "escape",
            TemplateSource = new Mock<ITemplateSource>().Object,
        };
        using var dest = new MemoryStream();
        await provider.ApplyAsync(context, data, dest);
        return Encoding.UTF8.GetString(dest.ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsync_NoOptions_WritesValuesAsIs()
    {
        // Stage
        var provider = new HandlebarsTemplateProvider([], [], [], TestLogger.CreateLogger<HandlebarsTemplateProvider>());

        // Test
        var result = await RenderAsync(provider, "{{value}}", new { value = "<b>&</b>" });

        // Assert
        Assert.AreEqual("<b>&</b>", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsync_NoEscapeOff_EncodesHtml()
    {
        // Stage
        var options = Options.Create(new HandlebarsOptions { NoEscape = false });
        var provider = new HandlebarsTemplateProvider([], [], [], TestLogger.CreateLogger<HandlebarsTemplateProvider>(), options);

        // Test
        var encoded = await RenderAsync(provider, "{{value}}", new { value = "<b>&</b>" });
        var raw = await RenderAsync(provider, "{{{value}}}", new { value = "<b>&</b>" });

        // Assert
        Assert.AreEqual("&lt;b&gt;&amp;&lt;/b&gt;", encoded);
        Assert.AreEqual("<b>&</b>", raw);
    }
}
