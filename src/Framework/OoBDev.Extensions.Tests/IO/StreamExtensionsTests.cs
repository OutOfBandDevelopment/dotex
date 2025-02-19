using OoBDev.Extensions.IO;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Extensions.Tests.IO;

[TestClass]
public class StreamExtensionsTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CopyOfTest()
    {
        using var ms = new MemoryStream(new byte[10 * 1024]);
        using var ms2 = ms.CopyOf();

        Assert.AreNotEqual(ms, ms2);
        Assert.AreEqual(ms.Length, ms2.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SplitStreamAsyncTest()
    {
        using var ms = new MemoryStream(new byte[3 * StreamExtensions.DefaultChunkLength]);
        var chunks = ms.SplitStreamAsync().ToBlockingEnumerable().ToList();
        Assert.AreEqual(3, chunks.Count);
    }
}
