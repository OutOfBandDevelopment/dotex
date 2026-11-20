using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.FileInventory.Cli.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class InventoryCommandTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Scan_FolderWithDuplicates_PrintsTheGroupAndExitsOne()
    {
        using var dir = new TempFileFactory(null!).GetTempDirectory();
        await File.WriteAllTextAsync(Path.Combine(dir.DirectoryPath, "a.txt"), "same content", TestContext.CancellationToken);
        Directory.CreateDirectory(Path.Combine(dir.DirectoryPath, "sub"));
        await File.WriteAllTextAsync(Path.Combine(dir.DirectoryPath, "sub", "b.txt"), "same content", TestContext.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(dir.DirectoryPath, "c.txt"), "different", TestContext.CancellationToken);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var code = await InventoryCommand.RunAsync(["scan", "--root", dir.DirectoryPath], output, error, CancellationToken.None);
        TestContext.WriteLine(output.ToString());

        Assert.AreEqual(1, code);
        StringAssert.Contains(output.ToString(), "scanned 3, hashed 3");
        StringAssert.Contains(output.ToString(), "duplicate group 1: 12 bytes, 2 files");
        StringAssert.Contains(output.ToString(), "1 duplicate group(s)");
        Assert.IsFalse(output.ToString().Contains("c.txt", global::System.StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Scan_FolderWithoutDuplicates_ExitsZero()
    {
        using var dir = new TempFileFactory(null!).GetTempDirectory();
        await File.WriteAllTextAsync(Path.Combine(dir.DirectoryPath, "a.txt"), "one", TestContext.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(dir.DirectoryPath, "b.txt"), "two", TestContext.CancellationToken);
        using var output = new StringWriter();

        var code = await InventoryCommand.RunAsync(["scan", "--root", dir.DirectoryPath], output, new StringWriter(), CancellationToken.None);

        Assert.AreEqual(0, code);
        StringAssert.Contains(output.ToString(), "0 duplicate group(s)");
    }

    [TestMethod]
    public async Task Images_TwoCopiesOfOnePicture_PrintsTheGroupAndExitsOne()
    {
        using var dir = new TempFileFactory(null!).GetTempDirectory();
        using var bitmap = new SkiaSharp.SKBitmap(200, 150);
        using (var canvas = new SkiaSharp.SKCanvas(bitmap))
        {
            canvas.Clear(SkiaSharp.SKColors.White);
            using var paint = new SkiaSharp.SKPaint { Color = SkiaSharp.SKColors.Black };
            canvas.DrawRect(0, 0, 60, 150, paint);
            canvas.DrawRect(120, 75, 80, 75, paint);
        }

        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        var bytes = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).ToArray();
        await File.WriteAllBytesAsync(Path.Combine(dir.DirectoryPath, "one.png"), bytes, TestContext.CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(dir.DirectoryPath, "two.png"), bytes, TestContext.CancellationToken);
        using var output = new StringWriter();

        var code = await InventoryCommand.RunAsync(["images", "--root", dir.DirectoryPath], output, new StringWriter(), CancellationToken.None);
        TestContext.WriteLine(output.ToString());

        Assert.AreEqual(1, code);
        StringAssert.Contains(output.ToString(), "near-duplicate group 1: 2 images");
    }

    [TestMethod]
    public async Task Run_BadArguments_ExitsTwoWithUsage()
    {
        using var error = new StringWriter();

        Assert.AreEqual(2, await InventoryCommand.RunAsync([], new StringWriter(), error, CancellationToken.None));
        StringAssert.Contains(error.ToString(), "file-inventory scan");

        using var error2 = new StringWriter();
        Assert.AreEqual(2, await InventoryCommand.RunAsync(["scan", "--root", Path.Combine(Path.GetTempPath(), "does-not-exist-" + global::System.Guid.NewGuid().ToString("N"))], new StringWriter(), error2, CancellationToken.None));
        StringAssert.Contains(error2.ToString(), "--root");
    }
}
