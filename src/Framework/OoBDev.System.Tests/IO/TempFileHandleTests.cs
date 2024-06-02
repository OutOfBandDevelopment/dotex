using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System.IO;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class TempFileHandleTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CreateTempFileHandleTest()
    {
        string? tempFileName = null;
        using (var temp = new TempFileHandle())
        {
            tempFileName = temp.FilePath;
            TestContext.WriteLine(temp.FilePath);
            Assert.IsTrue(File.Exists(tempFileName));
        }
        Assert.IsFalse(File.Exists(tempFileName));
    }
}
