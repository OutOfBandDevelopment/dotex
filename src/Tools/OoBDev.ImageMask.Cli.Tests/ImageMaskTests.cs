using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using System;
using System.IO;

namespace OoBDev.ImageMask.Cli.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class ImageMaskTests
{
    private string _dir = string.Empty;

    [TestInitialize]
    public void Setup() => _dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}")).FullName;

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, recursive: true);

    private string Image(string name, SKColor background, SKColor? clock = null)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(40, 20, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Erase(background);
        if (clock is { } c)
            for (var x = 30; x < 38; x++)
                bitmap.SetPixel(x, 2, c);

        var path = Path.Combine(_dir, name);
        ImageMasker.SavePng(bitmap, path);
        return path;
    }

    private static MaskDefinition ClockZone(string match = "*") => MaskDefinition.Parse(
        $$"""{ "zones": [ { "name": "clock", "match": "{{match}}", "x": 28, "y": 0, "width": 12, "height": 6 } ] } // clock""");

    [TestMethod]
    public void Parse_ZonesJson_ReadsBoundingBoxAndDefaults()
    {
        // Test
        var definition = MaskDefinition.Parse("""{ "zones": [ { "name": "clock", "x": 1, "y": 2, "width": 3, "height": 4 } ] }""");

        // Assert
        var zone = Assert.ContainsSingle(definition.Zones);
        Assert.AreEqual(("clock", "*", 1, 2, 3, 4, "#FF0000"), (zone.Name, zone.Match, zone.X, zone.Y, zone.Width, zone.Height, zone.Color));
    }

    [TestMethod]
    public void Apply_ZoneOutsideAndInside_PaintsOnlyTheZone()
    {
        // Stage
        using var bitmap = ImageMasker.Load(Image("a.png", SKColors.White));

        // Test
        ImageMasker.Apply(bitmap, ClockZone(), "a.png");

        // Assert
        Assert.AreEqual(new SKColor(255, 0, 0), bitmap.GetPixel(30, 2));
        Assert.AreEqual(SKColors.White, bitmap.GetPixel(10, 10));
    }

    [TestMethod]
    public void Apply_MatchDoesNotFitTheFileName_PaintsNothing()
    {
        // Stage
        using var bitmap = ImageMasker.Load(Image("a.png", SKColors.White));

        // Test
        ImageMasker.Apply(bitmap, ClockZone("tui-*.png"), "a.png");

        // Assert
        Assert.AreEqual(SKColors.White, bitmap.GetPixel(30, 2));
    }

    [TestMethod]
    public void Apply_InvalidColor_Throws()
    {
        // Stage
        using var bitmap = ImageMasker.Load(Image("a.png", SKColors.White));
        var definition = MaskDefinition.Parse("""{ "zones": [ { "name": "bad", "color": "not-a-color", "width": 1, "height": 1 } ] }""");

        // Test / Assert
        Assert.Throws<FormatException>(() => ImageMasker.Apply(bitmap, definition, "a.png"));
    }

    [TestMethod]
    public void Compare_DifferenceInsideMaskedZone_Matches()
    {
        // Stage
        using var expected = ImageMasker.Load(Image("e.png", SKColors.White, SKColors.Black));
        using var actual = ImageMasker.Load(Image("a.png", SKColors.White, SKColors.Gray));
        ImageMasker.Apply(expected, ClockZone(), "e.png");
        ImageMasker.Apply(actual, ClockZone(), "a.png");

        // Test
        var result = ImageComparer.Compare(expected, actual, 0, out var diff);
        diff?.Dispose();

        // Assert
        Assert.IsTrue(result.IsMatch());
    }

    [TestMethod]
    public void Compare_DifferenceOutsideMaskedZone_ReportsPixels()
    {
        // Stage
        using var expected = ImageMasker.Load(Image("e.png", SKColors.White));
        using var actual = ImageMasker.Load(Image("a.png", SKColors.White));
        actual.SetPixel(5, 15, SKColors.Black);

        // Test
        var result = ImageComparer.Compare(expected, actual, 0, out var diff);

        // Assert
        Assert.AreEqual(1, result.DifferentPixels);
        Assert.AreEqual(new SKColor(255, 0, 255), diff!.GetPixel(5, 15));
        Assert.IsFalse(result.IsMatch());
        diff.Dispose();
    }

    [TestMethod]
    public void Compare_WithinTolerance_Matches()
    {
        // Stage
        using var expected = ImageMasker.Load(Image("e.png", new SKColor(100, 100, 100)));
        using var actual = ImageMasker.Load(Image("a.png", new SKColor(102, 100, 100)));

        // Test
        var result = ImageComparer.Compare(expected, actual, 2, out var diff);
        diff?.Dispose();

        // Assert
        Assert.IsTrue(result.IsMatch());
    }

    [TestMethod]
    public void Compare_DifferentSizes_DoesNotMatch()
    {
        // Stage
        using var expected = ImageMasker.Load(Image("e.png", SKColors.White));
        using var actual = new SKBitmap(10, 10);

        // Test
        var result = ImageComparer.Compare(expected, actual, 0, out var diff);

        // Assert
        Assert.IsFalse(result.SizeMatches);
        Assert.IsNull(diff);
    }

    [TestMethod]
    public void Run_OnlyMaskedZoneChanged_KeepsTheCommittedUnmaskedImage()
    {
        // Stage
        var committed = Image("guide.png", SKColors.White, SKColors.Black);
        var capture = Image("new.png", SKColors.White, SKColors.Gray);
        var before = File.ReadAllBytes(committed);

        // Test
        var updated = ImageSync.Run(capture, committed, ClockZone(), 0, 0);

        // Assert
        Assert.IsFalse(updated);
        CollectionAssert.AreEqual(before, File.ReadAllBytes(committed));
    }

    [TestMethod]
    public void Run_RealChange_ReplacesTheCommittedImageWithTheUnmaskedCapture()
    {
        // Stage
        var committed = Image("guide.png", SKColors.White, SKColors.Black);
        var capture = Image("new.png", SKColors.Blue, SKColors.Gray);

        // Test
        var updated = ImageSync.Run(capture, committed, ClockZone(), 0, 0);

        // Assert
        Assert.IsTrue(updated);
        using var result = ImageMasker.Load(committed);
        Assert.AreEqual(SKColors.Gray, result.GetPixel(30, 2));
    }

    [TestMethod]
    public void Run_NoCommittedImage_CopiesTheCapture()
    {
        // Stage
        var capture = Image("new.png", SKColors.White);
        var committed = Path.Combine(_dir, "guide.png");

        // Test
        var updated = ImageSync.Run(capture, committed, new MaskDefinition(), 0, 0);

        // Assert
        Assert.IsTrue(updated);
        Assert.IsTrue(File.Exists(committed));
    }
}
