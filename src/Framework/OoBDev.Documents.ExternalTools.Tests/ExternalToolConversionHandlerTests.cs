using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.ExternalTools.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class ExternalToolConversionHandlerTests
{
    private sealed class FolderFactory : ITempFileFactory
    {
        public ITempFile GetTempFile() => throw new NotSupportedException();

        public ITempDirectory GetTempDirectory() => new Folder();
    }

    private sealed class Folder : ITempDirectory
    {
        public Folder() => DirectoryPath = Directory.CreateTempSubdirectory("externaltool").FullName;

        public string DirectoryPath { get; }

        public void Dispose() => Directory.Delete(DirectoryPath, true);
    }

    private static ExternalToolOptions Copy(string arguments = "/c copy /y {input} {output}") => new()
    {
        Name = "copy",
        Path = "cmd.exe",
        Arguments = arguments,
        Formats = "pcl",
        OutputExtension = "png",
    };

    private static void RequireWindows()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("The fake tool is cmd.exe.");
    }

    [TestMethod]
    public async Task ConvertAsync_CopyTool_WritesTheOutputToTheDestinationAsync()
    {
        // Stage
        RequireWindows();
        var handler = new ExternalToolConversionHandler(Copy(), new FolderFactory());
        using var source = new MemoryStream(Encoding.ASCII.GetBytes("plot data"));
        using var destination = new MemoryStream();

        // Test
        await handler.ConvertAsync(source, "application/vnd.hp-pcl", destination, "image/png");

        // Assert
        Assert.AreEqual("plot data", Encoding.ASCII.GetString(destination.ToArray()));
    }

    [TestMethod]
    public async Task ConvertAsync_DpiPlaceholder_IsSubstitutedAsync()
    {
        // Stage
        RequireWindows();
        var tool = Copy("/c echo {dpi}>{output}");
        tool.Dpi = 300;
        var handler = new ExternalToolConversionHandler(tool, new FolderFactory());
        using var source = new MemoryStream([1]);
        using var destination = new MemoryStream();

        // Test
        await handler.ConvertAsync(source, "application/vnd.hp-pcl", destination, "image/png");

        // Assert
        StringAssert.Contains(Encoding.ASCII.GetString(destination.ToArray()), "300");
    }

    [TestMethod]
    public async Task ConvertAsync_NonZeroExit_ThrowsWithTheExitCodeAsync()
    {
        // Stage
        RequireWindows();
        var handler = new ExternalToolConversionHandler(Copy("/c exit 7"), new FolderFactory());
        using var source = new MemoryStream([1]);
        using var destination = new MemoryStream();

        // Test
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => handler.ConvertAsync(source, "application/vnd.hp-pcl", destination, "image/png"));

        // Assert
        StringAssert.Contains(ex.Message, "exited with code 7");
    }

    [TestMethod]
    public async Task ConvertAsync_NoOutputFile_ThrowsAsync()
    {
        // Stage
        RequireWindows();
        var handler = new ExternalToolConversionHandler(Copy("/c exit 0"), new FolderFactory());
        using var source = new MemoryStream([1]);
        using var destination = new MemoryStream();

        // Test
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => handler.ConvertAsync(source, "application/vnd.hp-pcl", destination, "image/png"));

        // Assert
        StringAssert.Contains(ex.Message, "did not produce");
    }

    [TestMethod]
    public async Task ConvertAsync_MissingExecutable_ThrowsAsync()
    {
        // Stage
        var tool = Copy();
        tool.Path = Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid().ToString("N") + ".exe");
        var handler = new ExternalToolConversionHandler(tool, new FolderFactory());
        using var source = new MemoryStream([1]);
        using var destination = new MemoryStream();

        // Test
        var act = () => handler.ConvertAsync(source, "application/vnd.hp-pcl", destination, "image/png");

        // Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(act);
    }

    [TestMethod]
    public async Task ConvertAsync_NoPath_ThrowsAsync()
    {
        // Stage
        var tool = Copy();
        tool.Path = null;
        var handler = new ExternalToolConversionHandler(tool, new FolderFactory());
        using var source = new MemoryStream([1]);
        using var destination = new MemoryStream();

        // Test
        var act = () => handler.ConvertAsync(source, "application/vnd.hp-pcl", destination, "image/png");

        // Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(act);
    }

    [TestMethod]
    public async Task ConvertAsync_UnsupportedSource_ThrowsAsync()
    {
        // Stage
        var handler = new ExternalToolConversionHandler(Copy(), new FolderFactory());
        using var source = new MemoryStream([1]);
        using var destination = new MemoryStream();

        // Test
        var act = () => handler.ConvertAsync(source, "text/plain", destination, "image/png");

        // Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(act);
    }

    [TestMethod]
    public void Supported_EmptyFormats_AcceptsAnySource()
    {
        // Stage
        var tool = Copy();
        tool.Formats = string.Empty;
        var handler = new ExternalToolConversionHandler(tool, new FolderFactory());

        // Test
        var any = handler.SupportedSource("anything/at-all");
        var png = handler.SupportedDestination("image/png");

        // Assert
        Assert.IsTrue(any);
        Assert.IsTrue(png);
    }

    [TestMethod]
    public void Resolve_NamesAndExtensions_MapToContentTypes()
    {
        // Test
        var types = ContentTypeNames.Resolve("ps, .pcl;hpgl custom/type");

        // Assert
        CollectionAssert.AreEquivalent(new[] { "application/postscript", "application/vnd.hp-pcl", "image/vnd.hp-hpgl", "custom/type" }, types);
        Assert.AreEqual("pdf", ContentTypeNames.ExtensionFor("application/pdf"));
        Assert.AreEqual("bin", ContentTypeNames.ExtensionFor("x/y"));
    }
}
