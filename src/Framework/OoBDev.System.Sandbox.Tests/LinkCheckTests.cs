using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>A link inside a granted folder that points outside it must not widen the grant (REQ-003).</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class LinkCheckTests
{
    private static readonly TempFileFactory _temp = new(new ServiceCollection().BuildServiceProvider());

    private static GrantBroker Broker(string folder, out PluginManifest manifest)
    {
        manifest = new PluginManifest("links", "1.0.0", "1.0", new Dictionary<string, string>(), new PluginPermissions([new FilePermission(folder, "rw")], []), new Dictionary<string, string>());
        var store = new GrantStore(TimeProvider.System);
        store.Approve(manifest, manifest.Permissions, "owner");
        return new GrantBroker(store);
    }

    private static bool TryLink(string link, string target, bool directory)
    {
        try
        {
            if (directory)
            {
                Directory.CreateSymbolicLink(link, target);
            }
            else
            {
                File.CreateSymbolicLink(link, target);
            }

            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
        {
            return false;
        }
    }

    [TestMethod]
    public void CheckFile_FileLinkPointingOutside_IsRefused()
    {
        using var granted = _temp.GetTempDirectory();
        using var other = _temp.GetTempDirectory();
        var secret = Path.Combine(other.DirectoryPath, "secret.txt");
        File.WriteAllText(secret, "secret");
        if (!TryLink(Path.Combine(granted.DirectoryPath, "shortcut.txt"), secret, directory: false))
        {
            Assert.Inconclusive("Symbolic links cannot be created here.");
        }

        var broker = Broker(granted.DirectoryPath, out var manifest);

        var result = broker.CheckFile(manifest.Id, Path.Combine(granted.DirectoryPath, "shortcut.txt"), write: false);

        Assert.IsFalse(result.Allowed);
        Assert.AreEqual("outside the granted folders", result.Reason);
    }

    [TestMethod]
    public void CheckFile_FolderLinkPointingOutside_NewFileThroughItIsRefused()
    {
        using var granted = _temp.GetTempDirectory();
        using var other = _temp.GetTempDirectory();
        if (!TryLink(Path.Combine(granted.DirectoryPath, "door"), other.DirectoryPath, directory: true))
        {
            Assert.Inconclusive("Symbolic links cannot be created here.");
        }

        var broker = Broker(granted.DirectoryPath, out var manifest);

        var result = broker.CheckFile(manifest.Id, Path.Combine(granted.DirectoryPath, "door", "new.txt"), write: true);

        Assert.IsFalse(result.Allowed);
    }

    [TestMethod]
    public void CheckFile_LinkInsideTheGrantToAnotherGrantedFile_IsAllowed()
    {
        using var granted = _temp.GetTempDirectory();
        var real = Path.Combine(granted.DirectoryPath, "real.txt");
        File.WriteAllText(real, "ok");
        if (!TryLink(Path.Combine(granted.DirectoryPath, "alias.txt"), real, directory: false))
        {
            Assert.Inconclusive("Symbolic links cannot be created here.");
        }

        var broker = Broker(granted.DirectoryPath, out var manifest);

        var result = broker.CheckFile(manifest.Id, Path.Combine(granted.DirectoryPath, "alias.txt"), write: false);

        Assert.IsTrue(result.Allowed);
    }

    [TestMethod]
    public void Real_PathWithoutLinks_IsTheFullPath()
    {
        using var granted = _temp.GetTempDirectory();
        var path = Path.Combine(granted.DirectoryPath, "nothing", "yet.txt");

        Assert.AreEqual(Path.GetFullPath(path), LinkResolver.Real(path));
    }
}
