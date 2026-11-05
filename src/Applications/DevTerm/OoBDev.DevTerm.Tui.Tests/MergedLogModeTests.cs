using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
