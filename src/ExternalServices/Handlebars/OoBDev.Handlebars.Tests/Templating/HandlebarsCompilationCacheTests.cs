using OoBDev.Handlebars.Templating;
using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Handlebars.Tests.Templating;

[TestClass]
public class HandlebarsCompilationCacheTests
{
    private static HandlebarsTemplateProvider Create(HandlebarsOptions? options = null) =>
        new([], [], (IHelperDescriptor<HelperOptions>[])[], NullLogger<HandlebarsTemplateProvider>.Instance,
            Options.Create(options ?? new HandlebarsOptions()));

    private static async Task<string> RenderAsync(HandlebarsTemplateProvider provider, string template, object data)
    {
        var context = new TemplateContext
        {
            OpenTemplate = _ => new MemoryStream(Encoding.UTF8.GetBytes(template)),
            TemplateName = "cache",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "cache",
            TemplateSource = new Mock<ITemplateSource>().Object,
        };
        using var dest = new MemoryStream();
        await provider.ApplyAsync(context, data, dest);
        return Encoding.UTF8.GetString(dest.ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsync_SameTemplateTwice_CompilesOnceAndRendersEachData()
    {
        var provider = Create();

        var first = await RenderAsync(provider, "Hi {{name}}", new { name = "A" });
        var second = await RenderAsync(provider, "Hi {{name}}", new { name = "B" });

        Assert.AreEqual("Hi A", first);
        Assert.AreEqual("Hi B", second);
        Assert.AreEqual(1, provider.CachedTemplateCount);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsync_EditedTemplate_IsCompiledAgain()
    {
        var provider = Create();

        var before = await RenderAsync(provider, "v1 {{x}}", new { x = 1 });
        var after = await RenderAsync(provider, "v2 {{x}}", new { x = 1 });

        Assert.AreEqual("v1 1", before);
        Assert.AreEqual("v2 1", after);
        Assert.AreEqual(2, provider.CachedTemplateCount);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsync_CacheDisabled_KeepsNothing()
    {
        var provider = Create(new HandlebarsOptions { EnableCompilationCache = false });

        var result = await RenderAsync(provider, "{{x}}", new { x = 3 });

        Assert.AreEqual("3", result);
        Assert.AreEqual(0, provider.CachedTemplateCount);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsync_MoreTemplatesThanLimit_StaysBounded()
    {
        var provider = Create(new HandlebarsOptions { MaxCachedTemplates = 2 });

        for (var i = 0; i < 5; i++)
            await RenderAsync(provider, $"t{i}", new { });

        Assert.IsTrue(provider.CachedTemplateCount <= 2);
    }
}
