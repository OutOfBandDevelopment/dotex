using OoBDev.Handlebars.Helpers;
using OoBDev.Handlebars.Templating;
using OoBDev.System.Net.Mime;
using OoBDev.System.Providers;
using OoBDev.System.Security.Cryptography;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Handlebars.Tests.Templating;

[TestClass]
public class HandlebarsTemplateProviderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest_Simple()
    {
        var data = new
        {
        };
        var source = new MemoryStream();
        var writer = new StreamWriter(source, leaveOpen: true)
        {
            AutoFlush = true,
        };
        writer.Write("Hello World!");
        source.Position = 0;
        var dest = new MemoryStream();
        var reader = new StreamReader(dest);

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>(MockBehavior.Loose);
        var context = new TemplateContext
        {
            OpenTemplate = _ => source,
            TemplateName = "testName",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "TestReference",
            TemplateSource = mockSource.Object,
        };

        var provider = new HandlebarsTemplateProvider(
            [], [], [],
            TestLogger.CreateLogger<HandlebarsTemplateProvider>()
            );

        var result = await provider.ApplyAsync(context, data, dest);

        dest.Position = 0;
        var read = reader.ReadToEnd();

        TestContext.WriteLine(read);

        Assert.IsTrue(result);
        Assert.AreEqual("Hello World!", read);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest_WithDate()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            //TODO: fix this better
            Assert.Inconclusive();
            return;
        }

        var data = new
        {
        };
        var source = new MemoryStream();
        var writer = new StreamWriter(source, leaveOpen: true)
        {
            AutoFlush = true,
        };
        writer.Write("Now: {{date_now}}");
        source.Position = 0;
        var dest = new MemoryStream();
        var reader = new StreamReader(dest);

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>(MockBehavior.Loose);
        var mockDate = mockRepo.Create<IDateTimeProvider>();
        mockDate.Setup(s => s.Now).Returns(new DateTimeOffset(1, 2, 3, 4, 5, 6, new TimeSpan(5, 0, 0)));

        var context = new TemplateContext
        {
            OpenTemplate = _ => source,
            TemplateName = "testName",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "TestReference",
            TemplateSource = mockSource.Object,
        };

        var provider = new HandlebarsTemplateProvider(
            [], [], [new DateNowHelperDescriptor(mockDate.Object)],
            TestLogger.CreateLogger<HandlebarsTemplateProvider>()
            );

        var result = await provider.ApplyAsync(context, data, dest);

        dest.Position = 0;
        var read = reader.ReadToEnd();

        TestContext.WriteLine(read);

        Assert.IsTrue(result);
        Assert.AreEqual("Now: 2/3/0001 4:05:06 AM +05:00", read);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest_WithDateAndFormat()
    {
        var data = new
        {
        };
        var source = new MemoryStream();
        var writer = new StreamWriter(source, leaveOpen: true)
        {
            AutoFlush = true,
        };
        writer.Write("Today: {{date_now 'yyyy-MM-dd'}}");
        source.Position = 0;
        var dest = new MemoryStream();
        var reader = new StreamReader(dest);

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>(MockBehavior.Loose);
        var mockDate = mockRepo.Create<IDateTimeProvider>();
        mockDate.Setup(s => s.Now).Returns(new DateTimeOffset(1, 2, 3, 4, 5, 6, new TimeSpan(5, 0, 0)));

        var context = new TemplateContext
        {
            OpenTemplate = _ => source,
            TemplateName = "testName",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "TestReference",
            TemplateSource = mockSource.Object,
        };

        var provider = new HandlebarsTemplateProvider(
            [], [], [new DateNowHelperDescriptor(mockDate.Object)],
            TestLogger.CreateLogger<HandlebarsTemplateProvider>()
            );

        var result = await provider.ApplyAsync(context, data, dest);

        dest.Position = 0;
        var read = reader.ReadToEnd();

        TestContext.WriteLine(read);

        Assert.IsTrue(result);
        Assert.AreEqual("Today: 0001-02-03", read);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest_WithGuid()
    {
        var data = new
        {
        };
        var source = new MemoryStream();
        var writer = new StreamWriter(source, leaveOpen: true)
        {
            AutoFlush = true,
        };
        writer.Write("Guid :{{guid_new}}");
        source.Position = 0;
        var dest = new MemoryStream();
        var reader = new StreamReader(dest);

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>(MockBehavior.Loose);

        var mockGuid = mockRepo.Create<IGuidProvider>();
        mockGuid.Setup(s => s.NewGuid()).Returns(new Guid("8847D0E5-1ECA-4075-B0C1-E6BD606439D0"));

        var context = new TemplateContext
        {
            OpenTemplate = _ => source,
            TemplateName = "testName",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "TestReference",
            TemplateSource = mockSource.Object,
        };

        var provider = new HandlebarsTemplateProvider(
            [], [], [new GuidNewHelperDescriptor(mockGuid.Object)],
            TestLogger.CreateLogger<HandlebarsTemplateProvider>()
            );

        var result = await provider.ApplyAsync(context, data, dest);

        dest.Position = 0;
        var read = reader.ReadToEnd();

        TestContext.WriteLine(read);

        Assert.IsTrue(result);
        Assert.AreEqual("Guid :8847d0e5-1eca-4075-b0c1-e6bd606439d0", read);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest_WithHash()
    {
        var data = new
        {
            input = "hello!"
        };
        var source = new MemoryStream();
        var writer = new StreamWriter(source, leaveOpen: true)
        {
            AutoFlush = true,
        };
        writer.Write("Hash: {{hash input}}");
        source.Position = 0;
        var dest = new MemoryStream();
        var reader = new StreamReader(dest);

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>(MockBehavior.Loose);

        var mockGuid = mockRepo.Create<IHash>();
        mockGuid.Setup(s => s.GetHash(data.input)).Returns("FakeHash");

        var context = new TemplateContext
        {
            OpenTemplate = _ => source,
            TemplateName = "testName",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "TestReference",
            TemplateSource = mockSource.Object,
        };

        var provider = new HandlebarsTemplateProvider(
            [], [], [new HashHelperDescriptor(mockGuid.Object)],
            TestLogger.CreateLogger<HandlebarsTemplateProvider>()
            );

        var result = await provider.ApplyAsync(context, data, dest);

        dest.Position = 0;
        var read = reader.ReadToEnd();

        TestContext.WriteLine(read);

        Assert.IsTrue(result);
        Assert.AreEqual("Hash: FakeHash", read);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest_WithGetSet()
    {
        var data = new
        {
            input = "hello!"
        };
        var source = new MemoryStream();
        var writer = new StreamWriter(source, leaveOpen: true)
        {
            AutoFlush = true,
        };
        writer.Write("Capture: {{set 'key1' input}} -> Output: {{get 'key1'}}");
        source.Position = 0;
        var dest = new MemoryStream();
        var reader = new StreamReader(dest);

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>(MockBehavior.Loose);

        var context = new TemplateContext
        {
            OpenTemplate = _ => source,
            TemplateName = "testName",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "TestReference",
            TemplateSource = mockSource.Object,
        };

        var provider = new HandlebarsTemplateProvider(
            [], [], [
                new GetHelperDescriptor(TestLogger.CreateLogger<GetHelperDescriptor>()),
                new SetHelperDescriptor(TestLogger.CreateLogger<SetHelperDescriptor>()),
            ],
            TestLogger.CreateLogger<HandlebarsTemplateProvider>()
            );

        var result = await provider.ApplyAsync(context, data, dest);

        dest.Position = 0;
        var read = reader.ReadToEnd();

        TestContext.WriteLine(read);

        Assert.IsTrue(result);
        Assert.AreEqual("Capture:  -> Output: hello!", read);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest_WithStringReplace()
    {
        var data = new
        {
            input = "How are you today?",
            target = "today",
            replacement = "feeling",
        };
        var source = new MemoryStream();
        var writer = new StreamWriter(source, leaveOpen: true)
        {
            AutoFlush = true,
        };

        writer.Write("String Replace: {{str-replace input target replacement}}");
        source.Position = 0;
        var dest = new MemoryStream();
        var reader = new StreamReader(dest);

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>(MockBehavior.Loose);

        var context = new TemplateContext
        {
            OpenTemplate = _ => source,
            TemplateName = "testName",
            TemplateContentType = ContentTypesExtensions.Text.HandlebarsTemplate,
            TemplateFileExtension = ".hbs",
            TargetContentType = ContentTypesExtensions.Text.Markdown,
            TargetFileExtension = ".md",
            TemplateReference = "TestReference",
            TemplateSource = mockSource.Object,
        };

        var provider = new HandlebarsTemplateProvider(
            [], [], [
                new StringReplaceHelperDescriptor(),
            ],
            TestLogger.CreateLogger<HandlebarsTemplateProvider>()
            );

        var result = await provider.ApplyAsync(context, data, dest);

        dest.Position = 0;
        var read = reader.ReadToEnd();

        TestContext.WriteLine(read);

        Assert.IsTrue(result);
        Assert.AreEqual("String Replace: How are you feeling?", read);

        mockRepo.VerifyAll();

    }
}
