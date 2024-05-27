using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using BinaryDataDecoders.ToolKit.IO;
using BinaryDataDecoders.TestUtilities;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class PathNavigatorFactoryTests
{
    public TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public void ToNavigableTest()
    {
        var di = new DirectoryInfo("../../../../");
        var xpath = di.ToNavigable().CreateNavigator();
        TestContext.AddResult(xpath);
    }
}
