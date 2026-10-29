using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration.Tests;

[TestClass]
public static class TestEnvironment
{
    [AssemblyInitialize]
    public static void IsolateUserData(TestContext context)
    {
        var home = Path.Combine(Path.GetTempPath(), "devterm-config-tests-" + Guid.NewGuid().ToString("N"));
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, home);
    }
}
