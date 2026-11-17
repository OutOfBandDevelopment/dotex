using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class StreamExTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task AsTempFileAsyncTest()
    {
        var message = "HelloWorld!";
        string? tempFile = null;
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(message));
        using (var temp = await ms.AsTempFileAsync().ConfigureAwait(false))
        {
            tempFile = temp.FilePath;
            var read = File.ReadAllText(temp.FilePath);
            TestContext.WriteLine(read);
            Assert.AreEqual(message, read);
        }

        Assert.IsFalse(File.Exists(tempFile));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ReadAsStringAsync_Stream_ReturnsTextAndDisposesTheStream()
    {
        TestContext.WriteLine("Stage: a memory stream holding 'Hello'");
        var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello"));

        TestContext.WriteLine("Test: read it as a string");
        var text = await ms.ReadAsStringAsync().ConfigureAwait(false);

        TestContext.WriteLine($"Assert: text '{text}', stream CanRead={ms.CanRead}");
        Assert.AreEqual("Hello", text);
        Assert.IsFalse(ms.CanRead);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ReadAsStringAsync_NullStream_ReturnsNull()
    {
        TestContext.WriteLine("Test: read a null stream");
        Stream? stream = null;

        var text = await stream.ReadAsStringAsync().ConfigureAwait(false);

        Assert.IsNull(text);
    }
}
