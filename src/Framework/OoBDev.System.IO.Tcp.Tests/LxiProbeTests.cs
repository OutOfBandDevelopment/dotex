using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.Tcp.Tests;

[TestClass]
public sealed class LxiProbeTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ProbeAsync_RawScpiAndVxi11OnlyDevices_BecomeTcpAndVxi11Hits()
    {
        // Stage
        IReadOnlyList<LxiDevice> found = [new("10.0.0.5", 1024, 5555, "RIGOL,DG1062Z"), new("10.0.0.6", 2048, 0, "")];
        var probe = new LxiProbe((_, _, _) => Task.FromResult(found));

        // Test
        var hits = await probe.ProbeAsync(TimeSpan.FromSeconds(1), TestContext.CancellationToken);
        TestContext.WriteLine($"{hits.Count} hits: {string.Join("; ", hits.Select(h => h.Display))}");

        // Assert
        Assert.AreEqual(2, hits.Count);
        Assert.AreEqual(("tcp", 5555, "RIGOL,DG1062Z"), (hits[0].Transport, hits[0].Port, hits[0].DisplayName));
        Assert.AreEqual(("vxi11", 2048, "10.0.0.6"), (hits[1].Transport, hits[1].Port, hits[1].DisplayName));
        Assert.IsTrue(hits.All(h => h.Kind == "lxi" && h.Source == "lxi"));
    }
}
