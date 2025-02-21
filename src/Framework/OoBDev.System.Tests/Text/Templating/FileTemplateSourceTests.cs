using OoBDev.System.IO;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OoBDev.System.Tests.Text.Templating;

[TestClass]
public class FileTemplateSourceTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetTest()
    {
        using var tempFile = new TempFileHandle(TestLogger.CreateLogger<TempFileHandle>());
        var options = new FileTemplatingOptions
        {
            Priority = 100,
            SandboxPath = Path.GetDirectoryName(tempFile.FilePath),
            TemplatePath = Path.GetDirectoryName(tempFile.FilePath)!
        };

        this.TestContext.WriteLine($"Temp File: {tempFile}");

        var fileTemplates = new List<IFileType>
        {
            new FileType()
            {
                 ContentType = "text/plain",
                 Extension = Path.GetExtension(tempFile.FilePath),
                 IsTemplateType = true
            }
        };

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockOptions = mockRepo.Create<IOptions<FileTemplatingOptions>>();
        mockOptions.SetupGet(s => s.Value).Returns(options);

        var source = new FileTemplateSource(
            mockOptions.Object,
            fileTemplates,
            TestLogger.CreateLogger<FileTemplateSource>()
            );

        var templates = source.Get(Path.GetFileNameWithoutExtension(tempFile.FilePath));
        var template = templates.First();

        Assert.IsNotNull(template);

        mockRepo.VerifyAll();
    }
}
