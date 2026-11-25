using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.System.Tests.IO;

/// <summary>The delegate-backed output pipe and the <c>WriteAsync</c> extension.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class DeviceAdapterOutputTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Create_FlushHandsTheBytesToTheDelegate()
    {
        var received = new List<byte>();
        var writer = DeviceAdapterOutput.Create((data, _) =>
        {
            received.AddRange(data.ToArray());
            return Task.CompletedTask;
        });

        await writer.WriteAsync(new byte[] { 1, 2, 3 }, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, received);
    }

    [TestMethod]
    public async Task Create_DelegateFailureSurfacesFromTheWrite()
    {
        var writer = DeviceAdapterOutput.Create((_, _) => Task.FromException(new global::System.TimeoutException("no answer")));

        await Assert.ThrowsExactlyAsync<global::System.TimeoutException>(async () => await writer.WriteAsync(new byte[] { 1 }, TestContext.CancellationToken));
    }

    [TestMethod]
    public void Create_RejectsANullDelegate() =>
        Assert.ThrowsExactly<global::System.ArgumentNullException>(() => DeviceAdapterOutput.Create(null!));
}
