using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Configuration.Tests;

[TestClass]
public static class TestEnvironment
{
    private static TempDirectoryHandle? _home;

    [AssemblyInitialize]
    public static void IsolateUserData(TestContext context)
    {
        _home = new TempDirectoryHandle();
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, _home.DirectoryPath);
    }

    [AssemblyCleanup]
    public static void DeleteUserData() => _home?.Dispose();
}
