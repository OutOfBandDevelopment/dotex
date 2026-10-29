using System.Buffers;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Core.Tests;

/// <summary>The stream-to-pipe pump with in-memory streams.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class StreamToPipePumpTests
{
    [TestMethod]
    public async Task RunAsync_CopiesAllBytesAndCompletesTheWriter()
    {
        var pipe = new Pipe();
        using var source = new MemoryStream([1, 2, 3, 4]);

        await StreamToPipePump.RunAsync(source, pipe.Writer, CancellationToken.None);

        var read = await pipe.Reader.ReadAsync();
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, read.Buffer.ToArray());
        Assert.IsTrue(read.IsCompleted);
    }

    [TestMethod]
    public async Task RunAsync_ReadFailure_CompletesTheWriterWithTheError()
    {
        var pipe = new Pipe();
        await StreamToPipePump.RunAsync(new FailingStream(), pipe.Writer, CancellationToken.None);

        await Assert.ThrowsExactlyAsync<IOException>(async () => await pipe.Reader.ReadAsync());
    }

    [TestMethod]
    public async Task RunAsync_Cancelled_EndsWithoutAnError()
    {
        var pipe = new Pipe();
        using var cts = new CancellationTokenSource();
        using var source = new BlockingStream();

        var run = StreamToPipePump.RunAsync(source, pipe.Writer, cts.Token);
        await cts.CancelAsync();
        await run;

        var read = await pipe.Reader.ReadAsync();
        Assert.IsTrue(read.IsCompleted);
    }

    [TestMethod]
    public async Task RunAsync_NullArguments_Throw()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => StreamToPipePump.RunAsync(null!, new Pipe().Writer, CancellationToken.None));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => StreamToPipePump.RunAsync(new MemoryStream(), null!, CancellationToken.None));
    }
}
