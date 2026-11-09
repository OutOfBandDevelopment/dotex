using OoBDev.Handlebars.Helpers;
using OoBDev.Handlebars.Templating;
using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Handlebars.Tests.Templating;

[TestClass]
public class HandlebarsFormattingHelperTests
{
    private static async Task<string> RenderAsync(string template, object data)
    {
        IHelperDescriptor<HelperOptions>[] helpers =
        [
            new FormatDateHelperDescriptor(),
            new FormatNumberHelperDescriptor(),
            new FormatNumberHelperDescriptor("currency", "C"),
            new JsonHelperDescriptor(),
        ];
        var provider = new HandlebarsTemplateProvider([], [], helpers, TestLogger.CreateLogger<HandlebarsTemplateProvider>());
        var context = new TemplateContext
        {
            OpenTemplate = _ => new MemoryStream(Encoding.UTF8.GetBytes(template)),
            TemplateName = "format",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "format",
            TemplateSource = new Mock<ITemplateSource>().Object,
        };
        using var dest = new MemoryStream();
        await provider.ApplyAsync(context, data, dest);
        return Encoding.UTF8.GetString(dest.ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task FormatDate_DateAndText_UsesFormatAndDefault()
    {
        var result = await RenderAsync("{{format_date d \"dd/MM/yyyy\"}}|{{format_date d}}|{{format_date t}}|[{{format_date n}}]",
            new { d = new DateTime(2026, 11, 5), t = "2026-11-06T10:00:00+00:00", n = 5 });

        Assert.AreEqual("05/11/2026|2026-11-05|2026-11-06|[]", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task FormatNumber_DefaultFormatCultureAndNonNumber()
    {
        var result = await RenderAsync("{{format_number v}}|{{format_number v \"N0\" \"de-DE\"}}|{{format_number s}}",
            new { v = 1234.5m, s = "abc" });

        Assert.AreEqual("1,234.50|1.235|abc", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Currency_UsesCultureFormat()
    {
        var result = await RenderAsync("{{currency v \"C\" \"en-US\"}}", new { v = 12.5m });

        Assert.AreEqual("$12.50", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Json_SerializesCompactAndIndented()
    {
        var result = await RenderAsync("{{json o}}|{{json o true}}", new { o = new { A = 1 } });

        Assert.AreEqual("{\"A\":1}|{\n  \"A\": 1\n}".Replace("\n", Environment.NewLine), result);
    }
}
