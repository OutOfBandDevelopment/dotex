using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class ToolboxInspectTests
{
    private static FormFile File(byte[] bytes) => new(new MemoryStream(bytes), 0, bytes.Length, "content", "a.bin");

    [TestMethod]
    public async Task Inspect_Text_ReportsLengthHashAndDump()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var controller = new ToolboxController(provider);

        var result = (await controller.Inspect(File(Encoding.ASCII.GetBytes("hello world, 0123456789")))).Value!;

        Assert.AreEqual(23, result.Length);
        Assert.AreEqual("0000  68 65 6C 6C 6F 20 77 6F 72 6C 64 2C 20 30 31 32  hello world, 012", result.HexDump.Split('\n')[0]);
        Assert.AreEqual("0010  33 34 35 36 37 38 39                             3456789", result.HexDump.Split('\n')[1]);
        Assert.AreEqual(64, result.Sha256.Length);
        Assert.IsNull(result.DetectedContentType);
    }

    [TestMethod]
    public async Task Inspect_EmptyFile_HasEmptyDump()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        var result = (await new ToolboxController(provider).Inspect(File([]))).Value!;

        Assert.AreEqual(0, result.Length);
        Assert.AreEqual(string.Empty, result.HexDump);
    }
}
