using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Tests.IO;

[TestCategory("Unit")]
[TestClass]
public sealed class WriteDelayStreamTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task WriteAsync_Bytes_ReachTheInnerStreamInOrder()
    {
        using var inner = new MemoryStream();
        await using var stream = new WriteDelayStream(inner, 0);

        await stream.WriteAsync(new byte[] { 1, 2, 3 }, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, inner.ToArray());
    }

    [TestMethod]
    public async Task WriteAsync_WithDelay_WaitsBetweenBytes()
    {
        using var inner = new MemoryStream();
        await using var stream = new WriteDelayStream(inner, 30);
        var started = global::System.Diagnostics.Stopwatch.GetTimestamp();

        await stream.WriteAsync(new byte[] { 1, 2, 3 }, TestContext.CancellationToken);

        Assert.IsGreaterThanOrEqualTo(40, (int)global::System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
    }

    [TestMethod]
    public void Constructor_NegativeDelay_Throws()
    {
        using var inner = new MemoryStream();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new WriteDelayStream(inner, -1));
    }

    [TestMethod]
    public void Capabilities_AreWriteOnly()
    {
        using var stream = new WriteDelayStream(new MemoryStream(), 0);

        Assert.IsTrue(stream.CanWrite);
        Assert.IsFalse(stream.CanRead);
        Assert.IsFalse(stream.CanSeek);
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Length);
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Read(new byte[1], 0, 1));
    }
}
