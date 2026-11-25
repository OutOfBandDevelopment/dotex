using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Handlebars.Helpers;
using OoBDev.Handlebars.Templating;
using OoBDev.System.Accessors;
using OoBDev.System.Net.Mime;
using OoBDev.System.Providers;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Handlebars.Tests.Templating;

[TestClass]
public class HandlebarsTranslateHelperTests
{
    private sealed class TestAccessor : IAccessor<CultureInfo>
    {
        public CultureInfo? Value { get; set; }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Translate_RequestCulture_WritesTextOrKeyAsync()
    {
        var translations = new Mock<ITranslationProvider>();
        translations.Setup(t => t.Translate("greeting", It.Is<CultureInfo>(c => c.Name == "fr-CA"))).Returns("Allo");
        var accessor = new TestAccessor { Value = CultureInfo.GetCultureInfo("fr-CA") };
        IHelperDescriptor<HelperOptions>[] helpers = [new TranslateHelperDescriptor(translations.Object, accessor)];
        var provider = new HandlebarsTemplateProvider([], [], helpers, TestLogger.CreateLogger<HandlebarsTemplateProvider>());
        var context = new TemplateContext
        {
            OpenTemplate = _ => new MemoryStream(Encoding.UTF8.GetBytes("{{translate \"greeting\"}}|{{translate \"unknown\"}}")),
            TemplateName = "t",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "t",
            TemplateSource = new Mock<ITemplateSource>().Object,
        };
        using var dest = new MemoryStream();

        await provider.ApplyAsync(context, new { }, dest);

        Assert.AreEqual("Allo|unknown", Encoding.UTF8.GetString(dest.ToArray()));
    }
}
