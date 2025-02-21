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
}
