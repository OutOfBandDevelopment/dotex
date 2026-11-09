using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Hpgl.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class HpglToSvgConversionHandlerTests
{
    [TestMethod]
    public async Task ConvertAsync_Hpgl_WritesSvgAsync()
    {
        // Stage
        var handler = new HpglToSvgConversionHandler();
        using var source = new MemoryStream(Encoding.ASCII.GetBytes("IN;PU0,0;PD100,0,100,100;PU;"));
        using var destination = new MemoryStream();

        // Test
        await handler.ConvertAsync(source, "image/vnd.hp-hpgl", destination, "image/svg+xml");

        // Assert
        var svg = Encoding.UTF8.GetString(destination.ToArray());
        StringAssert.Contains(svg, "<svg");
        StringAssert.Contains(svg, "<path");
    }

    [TestMethod]
    public async Task ConvertAsync_UnsupportedSource_ThrowsAsync()
    {
        // Stage
        var handler = new HpglToSvgConversionHandler();
        using var source = new MemoryStream();
        using var destination = new MemoryStream();

        // Test
        var act = () => handler.ConvertAsync(source, "text/plain", destination, "image/svg+xml");

        // Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(act);
    }

    [TestMethod]
    public void Supported_ContentTypes_AreCaseInsensitive()
    {
        // Stage
        var handler = new HpglToSvgConversionHandler();

        // Test
        var source = handler.SupportedSource("IMAGE/VND.HP-HPGL");
        var svg = handler.SupportedDestination("image/svg+xml");
        var png = handler.SupportedDestination("image/png");

        // Assert
        Assert.IsTrue(source);
        Assert.IsTrue(svg);
        Assert.IsFalse(png);
    }
}
