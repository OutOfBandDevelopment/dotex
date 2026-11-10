using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>Grants and the audit trail survive a restart (REQ-002: stored and auditable).</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class GrantStoreTests
{
    private static readonly TempFileFactory _temp = new(new ServiceCollection().BuildServiceProvider());

    private static PluginManifest Manifest() => new(
        "stored",
        "1.0.0",
        "1.0",
        new Dictionary<string, string>(),
        new PluginPermissions([new FilePermission("data", "rw")], [new NetworkPermission("api.example.org", 443)]),
        new Dictionary<string, string>());

    [TestMethod]
    public void Approve_WithAPath_IsFoundByANewStoreWithTheSameHash()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Path.Combine(directory.DirectoryPath, "grants.json");
        var manifest = Manifest();
        var first = new GrantStore(TimeProvider.System, path).Approve(manifest, manifest.Permissions, "owner");

        var reloaded = new GrantStore(TimeProvider.System, path).Find("stored");

        Assert.IsNotNull(reloaded);
        Assert.AreEqual(first.Hash, reloaded.Hash);
        Assert.AreEqual("owner", reloaded.ApprovedBy);
    }

    [TestMethod]
    public void Revoke_WithAPath_StaysRevokedAfterReload()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Path.Combine(directory.DirectoryPath, "grants.json");
        var manifest = Manifest();
        var store = new GrantStore(TimeProvider.System, path);
        store.Approve(manifest, manifest.Permissions, "owner");
        store.Revoke("stored");

        Assert.IsNull(new GrantStore(TimeProvider.System, path).Find("stored"));
    }

    [TestMethod]
    public void Audit_WithAPath_IsKeptInOrderAcrossRestarts()
    {
        using var directory = _temp.GetTempDirectory();
        var path = Path.Combine(directory.DirectoryPath, "grants.json");
        var manifest = Manifest();
        var store = new GrantStore(TimeProvider.System, path);
        store.Approve(manifest, manifest.Permissions, "owner");
        new GrantBroker(store).CheckNetwork("stored", "evil.example.net", 443);

        var reloaded = new GrantStore(TimeProvider.System, path);
        reloaded.Revoke("stored");

        CollectionAssert.AreEqual(new[] { "approve", "net", "revoke" }, reloaded.Audit.Select(a => a.Action).ToArray());
        Assert.IsFalse(reloaded.Audit[1].Allowed);
    }

    [TestMethod]
    public void Approve_WithoutAPath_KeepsNothingOnDisk()
    {
        var manifest = Manifest();
        var store = new GrantStore(TimeProvider.System);

        store.Approve(manifest, manifest.Permissions, "owner");

        Assert.AreEqual(1, store.Audit.Count);
    }
}
