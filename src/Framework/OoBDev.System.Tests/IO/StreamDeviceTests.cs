using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Pipelines;
using OoBDev.TestUtilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.IO;

[TestClass]
[DoNotParallelize]
public class StreamDeviceTests
{
    private sealed class TextEncoder : IMessageEncoder<string>
    {
        public ReadOnlyMemory<byte> Encode(ref string request) => Encoding.UTF8.GetBytes(request);
    }

    private sealed class SendOnlyDevice : IDeviceDefinitionTransmitter<string>
    {
        public IMessageEncoder<string> Encoder { get; } = new TextEncoder();
    }

    private sealed class WrittenStream : MemoryStream
    {
        public event EventHandler? Wrote;

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await base.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            Wrote?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class FakeAdapter(Stream stream) : IDeviceAdapter
    {
        public string Type => "fake";

        public string Path => "fake";

        public Stream Stream { get; } = stream;

        public bool TryOpen(out Stream? opened)
        {
            opened = Stream;
            return true;
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Transmit_Message_WritesEncodedBytesAsync()
    {
        // Stage
        var stream = new WrittenStream();
        var written = new TaskCompletionSource();
        stream.Wrote += (_, _) => written.TrySetResult();
        using var cancel = new CancellationTokenSource();
        using var device = new StreamDevice<string>(new FakeAdapter(stream), new SendOnlyDevice(), 0, cancel.Token);

        // Test
        var accepted = await device.Transmit("hello").ConfigureAwait(false);
        await written.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await cancel.CancelAsync().ConfigureAwait(false);

        // Assert
        Assert.IsTrue(accepted);
        Assert.AreEqual("hello", Encoding.UTF8.GetString(stream.ToArray()));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Transmit_TwoMessages_WritesBothInOrderAsync()
    {
        // Stage
        var stream = new WrittenStream();
        var writes = 0;
        var both = new TaskCompletionSource();
        stream.Wrote += (_, _) =>
        {
            if (Interlocked.Increment(ref writes) == 2) both.TrySetResult();
        };
        using var cancel = new CancellationTokenSource();
        using var device = new StreamDevice<string>(new FakeAdapter(stream), new SendOnlyDevice(), 0, cancel.Token);

        // Test
        await device.Transmit("a").ConfigureAwait(false);
        await device.Transmit("b").ConfigureAwait(false);
        await both.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        await cancel.CancelAsync().ConfigureAwait(false);

        // Assert
        Assert.AreEqual("ab", Encoding.UTF8.GetString(stream.ToArray()));
    }

    private static async Task<TimeSpan> ProcessorTimeOverAsync(TimeSpan window)
    {
        var process = Process.GetCurrentProcess();
        process.Refresh();
        var before = process.TotalProcessorTime;
        await Task.Delay(window).ConfigureAwait(false);
        process.Refresh();
        return process.TotalProcessorTime - before;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Idle_NothingToSend_AddsNoProcessorTimeAsync()
    {
        // Stage
        var window = TimeSpan.FromMilliseconds(700);
        var background = await ProcessorTimeOverAsync(window).ConfigureAwait(false);
        using var cancel = new CancellationTokenSource();
        using var device = new StreamDevice<string>(new FakeAdapter(new WrittenStream()), new SendOnlyDevice(), 0, cancel.Token);
        await Task.Delay(200).ConfigureAwait(false);

        // Test
        var withDevice = await ProcessorTimeOverAsync(window).ConfigureAwait(false);
        await cancel.CancelAsync().ConfigureAwait(false);

        // Assert
        var added = withDevice - background;
        Assert.IsLessThan(TimeSpan.FromMilliseconds(300), added, $"An idle device added {added.TotalMilliseconds:F0} ms of processor time in 700 ms (background {background.TotalMilliseconds:F0} ms).");
    }
}
