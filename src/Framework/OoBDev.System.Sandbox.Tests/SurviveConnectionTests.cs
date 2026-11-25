using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>REQ-011 Survive: the plugin outlives its caller and a new caller reconnects over the named pipe with a new session key.</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class SurviveConnectionTests
{
    private static readonly FrameFlags _flags = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign;

    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void Reconnect_AfterCallerLeaves_SameInstanceAnswersWithNewSession()
    {
        // Stage
        var pipe = $"oobd-survive-{Guid.NewGuid():N}";
        using var plugin = new PlainPlugin(TestPlugin.Dll, $"survive {pipe} 20");
        TestContext.WriteLine($"Plugin listening on {pipe}");

        // Test
        string first;
        using (var one = new SurviveConnection(pipe, RandomNumberGenerator.GetBytes(32), _flags, TimeSpan.FromSeconds(15)))
        {
            var hello = one.Receive();
            one.Request("echo", """{"n":1}""");
            first = hello.Topic + hello.Text + one.Receive().Text;
        }

        using var two = new SurviveConnection(pipe, RandomNumberGenerator.GetBytes(32), _flags, TimeSpan.FromSeconds(15));
        var again = two.Receive();
        two.Request("echo", """{"n":2}""");
        var echoed = two.Receive();

        // Assert
        TestContext.WriteLine($"{first} | {again.Topic} {again.Text} {echoed.Text}");
        Assert.AreEqual("""lifecycle.reconnected{"session":1}{"n":1}""", first);
        Assert.AreEqual("""{"session":2}""", again.Text);
        Assert.AreEqual("""{"n":2}""", echoed.Text);
        Assert.IsFalse(plugin.WaitForExit(0), "The plugin is still the same running process.");
    }

    [TestMethod]
    public void Window_NoCallerReturns_PluginExits()
    {
        // Stage
        using var plugin = new PlainPlugin(TestPlugin.Dll, $"survive oobd-survive-{Guid.NewGuid():N} 1");

        // Test and Assert
        Assert.IsTrue(plugin.WaitForExit(15000), "The plugin ends when the reconnect window passes.");
        Assert.AreEqual(0, plugin.ExitCode);
    }
}
