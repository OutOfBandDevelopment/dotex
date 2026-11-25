using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.Sandbox.Windows;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>Folder access given to the AppContainer is removed again by <see cref="AppContainerLauncher.Revoke"/>.</summary>
[TestCategory("Integration")]
[TestClass]
public sealed class AppContainerRevokeTests
{
    private static readonly TempFileFactory _temp = new(new ServiceCollection().BuildServiceProvider());

    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void Revoke_AfterGrant_RemovesTheContainerRule()
    {
        // Stage
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("AppContainer is Windows only.");
        }

        using var directory = _temp.GetTempDirectory();
        var sid = AppContainerLauncher.ContainerSid();
        int Rules() => new DirectoryInfo(directory.DirectoryPath).GetAccessControl().GetAccessRules(true, false, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>().Count(r => r.IdentityReference.Equals(sid));
        AppContainerLauncher.Grant(directory.DirectoryPath, write: true);
        var granted = Rules();

        // Test
        var removed = AppContainerLauncher.Revoke(directory.DirectoryPath);
        var again = AppContainerLauncher.Revoke(directory.DirectoryPath);

        // Assert
        TestContext.WriteLine($"Rules after grant {granted}, revoked {removed}, second revoke {again}, rules now {Rules()}");
        Assert.IsTrue(granted > 0);
        Assert.IsTrue(removed);
        Assert.IsFalse(again);
        Assert.AreEqual(0, Rules());
    }
}
