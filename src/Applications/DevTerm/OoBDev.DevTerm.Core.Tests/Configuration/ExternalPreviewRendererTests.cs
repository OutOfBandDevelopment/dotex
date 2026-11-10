using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Configuration.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class ExternalPreviewRendererTests
{
    private string _directory = string.Empty;

    [TestInitialize]
    public void Initialize() => _directory = Directory.CreateTempSubdirectory("devterm-preview-").FullName;

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    internal static StreamMonitorCapture Capture(StreamContentKind kind, byte[] data) =>
        new(new StreamCapture(kind, data, DateTimeOffset.UnixEpoch, StreamCaptureEnd.Complete, false), "dev", DateTimeOffset.UnixEpoch, null, null);

    [TestMethod]
    public void FindTool_PostScript_FindsGhostscriptInASearchFolderWithSaferArguments()
    {
        File.WriteAllText(Path.Combine(_directory, "gswin64c.exe"), string.Empty);

        var tool = new ExternalPreviewRenderer([_directory]).FindTool(StreamContentKind.PostScript);

        Assert.IsNotNull(tool);
        Assert.AreEqual(Path.Combine(_directory, "gswin64c.exe"), tool.Path);
        StringAssert.Contains(tool.Arguments, "-dSAFER");
    }

    [TestMethod]
    public void FindTool_Pcl_WantsGhostPclNotGhostscript()
    {
        File.WriteAllText(Path.Combine(_directory, "gswin64c.exe"), string.Empty);

        Assert.IsNull(new ExternalPreviewRenderer([_directory]).FindTool(StreamContentKind.Pcl));
    }

    [TestMethod]
    public async Task RenderAsync_NoToolInstalled_SaysWhichToInstall()
    {
        var (png, error) = await new ExternalPreviewRenderer([_directory]).RenderAsync(Capture(StreamContentKind.PostScript, "%!PS\nshowpage\n"u8.ToArray()));

        Assert.IsNull(png);
        StringAssert.Contains(error, "Install Ghostscript");
    }

    [TestMethod]
    public async Task RenderAsync_OtherFormat_IsNotHandled()
    {
        var (png, error) = await new ExternalPreviewRenderer([_directory]).RenderAsync(Capture(StreamContentKind.Hpgl, "IN;"u8.ToArray()));

        Assert.IsNull(png);
        StringAssert.StartsWith(error, "No external preview");
    }
}

[TestClass]
[TestCategory("Integration")]
public sealed class ExternalPreviewRendererIntegrationTests
{
    [TestMethod]
    public async Task RenderAsync_RealGhostscript_DrawsAPng()
    {
        var renderer = new ExternalPreviewRenderer();
        if (renderer.FindTool(StreamContentKind.PostScript) is null)
        {
            Assert.Inconclusive("Ghostscript is not installed.");
        }

        var (png, error) = await renderer.RenderAsync(ExternalPreviewRendererTests.Capture(StreamContentKind.PostScript, "%!PS\n100 100 moveto 300 300 lineto stroke showpage\n"u8.ToArray()));

        Assert.IsNotNull(png, error);
        Assert.AreEqual(0x89, png[0]);
    }
}
