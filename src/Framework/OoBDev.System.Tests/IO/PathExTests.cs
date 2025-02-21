using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class PathExTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public void EnumerateFilesTest()
    {
        var wildcardPath = @"C:\Repos\**\src\**\*.Tests\*\*.cs";
        // var wildcardPath = @"C:\Repos\mwwhited\BinaryDataDecoders\src\**\*.Tests\*\*.cs";

        foreach (var file in PathEx.EnumerateFiles(wildcardPath))
        {
            TestContext.WriteLine(file);
        }
    }
}
