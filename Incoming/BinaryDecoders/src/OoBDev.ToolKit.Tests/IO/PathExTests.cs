using OoBDev.TestUtilities;
using OoBDev.ToolKit.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.ToolKit.Tests.IO;

[TestClass]
public class PathExTests
{
    public TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public void EnumerateFilesTest()
    {
       var wildcardPath = @"C:\Repos\**\src\**\*.Tests\*\*.cs";
       // var wildcardPath = @"C:\Repos\mwwhited\OoBDev\src\**\*.Tests\*\*.cs";

        foreach (var file in PathEx.EnumerateFiles(wildcardPath))
        {
            this.TestContext.WriteLine(file);
        }
    }
}
