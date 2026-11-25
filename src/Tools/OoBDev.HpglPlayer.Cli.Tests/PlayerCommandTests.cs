using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.HpglPlayer.Cli.Tests;

[TestClass]
public sealed class PlayerCommandTests
{
    private static readonly byte[] _square = "IN;SP1;PU1000,1000;PD5000,1000,5000,5000,1000,5000,1000,1000;PU;"u8.ToArray();

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public void Run_PlotFile_HandsAStereoWavToThePlayer()
    {
        // Stage
        var path = Path.Combine(AppContext.BaseDirectory, $"square_{Guid.NewGuid():N}.plt");
        File.WriteAllBytes(path, _square);
        byte[]? played = null;
        try
        {
            // Test
            var code = PlayerCommand.Run([path, "--seconds", "0.1"], (wav, _) => played = wav, TextWriter.Null);
            TestContext.WriteLine($"exit={code} bytes={played?.Length}");

            // Assert
            Assert.AreEqual(0, code);
            Assert.IsNotNull(played);
            Assert.AreEqual("RIFF", Encoding.ASCII.GetString(played, 0, 4));
            Assert.AreEqual(2, BitConverter.ToInt16(played, 22));
            Assert.AreEqual(44 + (4800 * 4), played.Length);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Run_ZAndDevice_HandsAThreeChannelWavAndTheDeviceToThePlayer()
    {
        // Stage
        var path = Path.Combine(AppContext.BaseDirectory, $"square_{Guid.NewGuid():N}.plt");
        File.WriteAllBytes(path, _square);
        byte[]? played = null;
        string? device = null;
        try
        {
            // Test
            var code = PlayerCommand.Run([path, "--seconds", "0.1", "--z", "true", "--device", "Scope"], (wav, name) => { played = wav; device = name; }, TextWriter.Null);
            TestContext.WriteLine($"exit={code} channels={(played is null ? 0 : BitConverter.ToInt16(played, 22))} device={device}");

            // Assert
            Assert.AreEqual(0, code);
            Assert.IsNotNull(played);
            Assert.AreEqual(3, BitConverter.ToInt16(played, 22));
            Assert.AreEqual("Scope", device);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Run_WavOption_WritesTheFileAndDoesNotPlay()
    {
        // Stage
        var path = Path.Combine(AppContext.BaseDirectory, $"square_{Guid.NewGuid():N}.plt");
        var wavPath = Path.ChangeExtension(path, ".wav");
        File.WriteAllBytes(path, _square);
        var played = false;
        try
        {
            // Test
            var code = PlayerCommand.Run([path, "--seconds", "0.1", "--wav", wavPath], (_, _) => played = true, TextWriter.Null);

            // Assert
            Assert.AreEqual(0, code);
            Assert.IsFalse(played);
            Assert.IsTrue(File.Exists(wavPath));
        }
        finally
        {
            File.Delete(path);
            File.Delete(wavPath);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Run_NoArguments_PrintsUsage()
    {
        // Test
        using var error = new StringWriter();
        var code = PlayerCommand.Run([], (_, _) => { }, error);

        // Assert
        Assert.AreEqual(2, code);
        Assert.Contains("hpgl-player", error.ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Run_EmptyPlot_ReportsNothingToDraw()
    {
        // Stage
        var path = Path.Combine(AppContext.BaseDirectory, $"empty_{Guid.NewGuid():N}.plt");
        File.WriteAllBytes(path, "IN;"u8.ToArray());
        try
        {
            // Test
            using var error = new StringWriter();
            var code = PlayerCommand.Run([path], (_, _) => Assert.Fail("Nothing should play."), error);

            // Assert
            Assert.AreEqual(2, code);
            Assert.Contains("nothing to draw", error.ToString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Run_MissingFile_ReportsTheErrorWithoutThrowing()
    {
        // Test
        using var error = new StringWriter();
        var code = PlayerCommand.Run([Path.Combine(AppContext.BaseDirectory, "missing.plt")], (_, _) => { }, error);

        // Assert
        Assert.AreEqual(2, code);
        Assert.IsGreaterThan(0, error.ToString().Length);
    }
}
