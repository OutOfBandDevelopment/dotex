using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
namespace OoBDev.DevTerm.Tui.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class MergedLogModeTests
{
    [TestMethod]
    public void Render_Empty_SaysNothingRecordedYet()
    {
        StringAssert.Contains(MergedLogMode.Render(new MergedSessionLog()), "no traffic yet");
    }
}
