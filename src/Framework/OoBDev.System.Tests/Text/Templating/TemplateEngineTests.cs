using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.Text.Templating;

[TestClass]
public class TemplateEngineTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetAllTest()
    {
        var templateName = "test template";

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>();
        var mockContext = mockRepo.Create<ITemplateContext>();
        mockSource.Setup(s => s.Get(templateName)).Returns([mockContext.Object]);
        mockContext.Setup(s => s.Priority).Returns(0);

        var template = new TemplateEngine([mockSource.Object], [], TestLogger.CreateLogger<TemplateEngine>());

        var results = template.GetAll(templateName);

        Assert.AreEqual(1, results.Count());

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetTest()
    {
        var templateName = "test template";

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>();
        var mockContext = mockRepo.Create<ITemplateContext>();
        mockSource.Setup(s => s.Get(templateName)).Returns([mockContext.Object]);
        mockContext.Setup(s => s.Priority).Returns(0);

        var template = new TemplateEngine([mockSource.Object], [], TestLogger.CreateLogger<TemplateEngine>());

        var results = template.Get(templateName);

        Assert.AreEqual(mockContext.Object, results);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest_StringData_and_StringDataStream()
    {
        var templateName = "test template";
        var testData = new { };

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>();
        var mockProvider = mockRepo.Create<ITemplateProvider>();
        var mockContext = mockRepo.Create<ITemplateContext>();

        mockSource.Setup(s => s.Get(templateName)).Returns([mockContext.Object]);
        mockContext.Setup(s => s.Priority).Returns(0);
        mockProvider.Setup(s => s.CanApply(mockContext.Object)).Returns(true);
        mockProvider.Setup(s => s.ApplyAsync(
            It.Is<ITemplateContext>(v => v == mockContext.Object),
            It.Is<object>(v => v == testData),
            It.IsAny<Stream>()
            )).Returns(Task.FromResult(true))
            .Callback<ITemplateContext, object, Stream>((c, d, s) =>
            {
                var writer = new StreamWriter(s, leaveOpen: true) { AutoFlush = true };
                writer.Write("Fin!");
            });

        var template = new TemplateEngine(
            [mockSource.Object],
            [mockProvider.Object],
            TestLogger.CreateLogger<TemplateEngine>()
            );

        var results = await template.ApplyAsync(templateName, testData);

        Assert.AreEqual("Fin!", results);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest_ContextData_and_ContextDataStream()
    {
        var testData = new { };

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSource = mockRepo.Create<ITemplateSource>();
        var mockProvider = mockRepo.Create<ITemplateProvider>();
        var mockContext = mockRepo.Create<ITemplateContext>();

        mockProvider.Setup(s => s.CanApply(mockContext.Object)).Returns(true);
        mockProvider.Setup(s => s.ApplyAsync(
            It.Is<ITemplateContext>(v => v == mockContext.Object),
            It.Is<object>(v => v == testData),
            It.IsAny<Stream>()
            )).Returns(Task.FromResult(true))
            .Callback<ITemplateContext, object, Stream>((c, d, s) =>
            {
                var writer = new StreamWriter(s, leaveOpen: true) { AutoFlush = true };
                writer.Write("Fin!");
            });

        var template = new TemplateEngine(
            [mockSource.Object],
            [mockProvider.Object],
            TestLogger.CreateLogger<TemplateEngine>()
            );

        var results = await template.ApplyAsync(mockContext.Object, testData);

        Assert.AreEqual("Fin!", results);

        mockRepo.VerifyAll();
    }
}
