using OoBDev.System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Loopback.Tests;

/// <summary>The scripted loopback transport, driven directly.</summary>
[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Loopback)]
public sealed class LoopbackTransportTests
{
    private static LoopbackTransport Create(int intervalMs = 0) =>
        new(Options.Create(new LoopbackTransportOptions { SampleIntervalMs = intervalMs }));

    // One reader per transport: it buffers ahead, so a fresh one per call would drop lines.
    private static StreamReader Reader(LoopbackTransport transport) => new(transport.Input.AsStream(), Encoding.ASCII);

    private static async Task<string> Line(StreamReader reader) =>
        await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5)) ?? throw new InvalidOperationException("No response line.");

    private static async Task<LoopbackTransport> Open(int intervalMs = 0)
    {
        var transport = Create(intervalMs);
        await transport.OpenAsync();
        return transport;
    }

    private static Task Send(LoopbackTransport transport, string text) =>
        transport.WriteAsync(Encoding.ASCII.GetBytes(text + "\r\n"));

    [TestMethod]
    public async Task OpenAsync_Closed_BecomesOpenAndRaisesTheEvent()
    {
        var transport = Create();
        var changes = new List<ConnectionState>();
        transport.StateChanged += (_, e) => changes.Add(e.Current);

        await transport.OpenAsync();

        Assert.AreEqual(ConnectionState.Open, transport.State);
        CollectionAssert.AreEqual(new[] { ConnectionState.Open }, changes);
    }

    [TestMethod]
    public async Task CloseAsync_Open_BecomesClosed()
    {
        var transport = await Open();

        await transport.CloseAsync();

        Assert.AreEqual(ConnectionState.Closed, transport.State);
    }

    [TestMethod]
    public async Task OpenAsync_AfterClose_ExchangesDataAgain()
    {
        var transport = await Open();
        await transport.CloseAsync();
        await transport.OpenAsync();

        await Send(transport, "hello");

        Assert.AreEqual("From Loopback test", await Line(Reader(transport)));
    }

    [TestMethod]
    public async Task WriteAsync_Hello_AnswersCaseInsensitively()
    {
        var transport = await Open();

        await Send(transport, "HeLLo");

        Assert.AreEqual("From Loopback test", await Line(Reader(transport)));
    }

    [TestMethod]
    public async Task WriteAsync_SendStream_ReturnsARepeatingAlphabetRun()
    {
        var transport = await Open();

        await Send(transport, "Send Stream: 30, ascii");

        Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXYZABCD", await Line(Reader(transport)));
    }

    [TestMethod]
    public async Task WriteAsync_SendEvents_ReturnsOneLinePerEvent()
    {
        var transport = await Open();
        var reader = Reader(transport);

        await Send(transport, "Send Events: 3");

        Assert.AreEqual("Event 1", await Line(reader));
        Assert.AreEqual("Event 2", await Line(reader));
        Assert.AreEqual("Event 3", await Line(reader));
    }

    [TestMethod]
    public async Task WriteAsync_Meas_AdvancesTheSampleCounter()
    {
        var transport = await Open();
        var reader = Reader(transport);

        await Send(transport, "MEAS?");
        await Send(transport, "MEAS?");

        Assert.AreEqual(LoopbackGenerators.SensorSample(0), await Line(reader));
        Assert.AreEqual(LoopbackGenerators.SensorSample(1), await Line(reader));
    }

    [TestMethod]
    public async Task WriteAsync_Samples_ContinuesFromTheCounter()
    {
        var transport = await Open();
        var reader = Reader(transport);
        await Send(transport, "MEAS?");
        await Line(reader);

        await Send(transport, "Samples: 2");

        Assert.AreEqual(LoopbackGenerators.SensorSample(1), await Line(reader));
        Assert.AreEqual(LoopbackGenerators.SensorSample(2), await Line(reader));
    }

    [TestMethod]
    public async Task WriteAsync_Status_ReturnsLabelledReadings()
    {
        var transport = await Open();

        await Send(transport, "STATUS?");

        Assert.AreEqual("temp=21.50 C volts=3.30 V state=OK", await Line(Reader(transport)));
    }

    [TestMethod]
    public async Task WriteAsync_Help_ListsTheCommands()
    {
        var transport = await Open();
        var reader = Reader(transport);

        await Send(transport, "?");

        foreach (var expected in LoopbackScript.HelpLines) Assert.AreEqual(expected, await Line(reader));
    }

    [TestMethod]
    public async Task WriteAsync_UnknownCommand_ReportsItVisibly()
    {
        var transport = await Open();

        await Send(transport, "frobnicate");

        Assert.AreEqual("? Unrecognized: frobnicate", await Line(Reader(transport)));
    }

    [TestMethod]
    public async Task WriteAsync_EmptyLine_ProducesNoResponse()
    {
        var transport = await Open();
        var reader = Reader(transport);

        await Send(transport, "");
        await Send(transport, "hello");

        Assert.AreEqual("From Loopback test", await Line(reader));
    }

    [TestMethod]
    public async Task WriteAsync_StreamingWithInterval_PacesBetweenLines()
    {
        var transport = await Open(intervalMs: 60);

        var started = DateTime.UtcNow;
        await Send(transport, "Send Events: 3");

        Assert.IsTrue(DateTime.UtcNow - started >= TimeSpan.FromMilliseconds(100), "two gaps of 60 ms expected");
    }

    [TestMethod]
    public void Constructor_NullOptions_Throws() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new LoopbackTransport(null!));

    [TestMethod]
    public void Validator_NegativeInterval_Fails()
    {
        var result = new LoopbackTransportOptionsValidator().Validate(null, new LoopbackTransportOptions { SampleIntervalMs = -1 });

        Assert.IsTrue(result.Failed);
    }

    [TestMethod]
    public void Validator_Zero_Succeeds()
    {
        var result = new LoopbackTransportOptionsValidator().Validate(null, new LoopbackTransportOptions());

        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    public async Task TryAddLoopbackTransport_ResolvesAWorkingTransport()
    {
        await using var provider = new ServiceCollection().TryAddLoopbackTransport().BuildServiceProvider();

        var transport = provider.GetRequiredService<IDeviceAdapter>();
        await transport.OpenAsync();

        Assert.IsInstanceOfType<LoopbackTransport>(transport);
        Assert.AreEqual(ConnectionState.Open, transport.State);
    }

    [TestMethod]
    public void Generators_AreDeterministic()
    {
        Assert.AreEqual(LoopbackGenerators.SensorSample(5), LoopbackGenerators.SensorSample(5));
        Assert.AreEqual("WARN", LoopbackGenerators.Status(4).Split('=').Last());
    }
}
