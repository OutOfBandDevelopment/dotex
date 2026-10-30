using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Tui.Tests;

[TestClass]
public static class TestEnvironment
{
    [AssemblyInitialize]
    public static void IsolateUserData(TestContext context)
    {
        var home = Path.Combine(Path.GetTempPath(), "devterm-tui-tests-" + Guid.NewGuid().ToString("N"));
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, home);
    }
}
