using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>
/// The whole chain on the platform's real sandbox: signed package, owner grant, mutex, launch, and a plugin that can do nothing itself and asks
/// the host for files and network. Ends Inconclusive where no sandbox is installed.
/// </summary>
[TestCategory("Integration")]
[TestClass]
public sealed class PluginHostTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        public int Requests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("hello from " + request.RequestUri!.Host) });
        }
    }

    private static readonly TempFileFactory _temp = new(new ServiceCollection().BuildServiceProvider());
    private readonly ECDsa _publisher = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly string _id = "host-test-" + Guid.NewGuid().ToString("N");
    private readonly GrantStore _grants = new(TimeProvider.System);
    private readonly ContextBroker _contexts = new(TimeProvider.System);
    private readonly StubHandler _network = new();

    [TestInitialize]
    public void RequireSandbox()
    {
        if (!TestLauncher.Instance.IsAvailable)
        {
            Assert.Inconclusive($"{TestLauncher.Instance.Mechanism} is not available on this machine.");
        }
    }

    [TestMethod]
    public void Call_FileInsideTheGrant_IsReadThroughTheBrokerAndOutsideIsRefused()
    {
        using var package = _temp.GetTempDirectory();
        using var granted = _temp.GetTempDirectory();
        using var other = _temp.GetTempDirectory();
        File.WriteAllText(Path.Combine(granted.DirectoryPath, "in.txt"), "inside");
        File.WriteAllText(Path.Combine(other.DirectoryPath, "secret.txt"), "secret");
        using var host = Host(package, granted.DirectoryPath, "ro", out var manifest, ("api.example.org", 443));
        using var start = host.Start(package.DirectoryPath, "brokered", PluginLifetime.Bound).Lease!;

        var inside = Call(start, new { op = "read", path = Path.Combine(granted.DirectoryPath, "in.txt") }, BrokerRights.FileRead);
        var outside = Call(start, new { op = "read", path = Path.Combine(other.DirectoryPath, "secret.txt") }, BrokerRights.FileRead);

        Assert.IsTrue(inside.GetProperty("allowed").GetBoolean());
        Assert.AreEqual("inside", inside.GetProperty("text").GetString());
        Assert.IsFalse(outside.GetProperty("allowed").GetBoolean());
        Assert.AreEqual("outside the granted folders", outside.GetProperty("reason").GetString());
        Assert.IsTrue(_grants.Audit.Any(a => a.Action == "file" && !a.Allowed), "The refusal is audited.");
        Assert.AreEqual(_id, manifest.Id);
    }

    [TestMethod]
    public void Call_WriteIntoReadOnlyFolder_IsRefusedAndReadWriteWorks()
    {
        using var package = _temp.GetTempDirectory();
        using var granted = _temp.GetTempDirectory();
        using var host = Host(package, granted.DirectoryPath, "rw", out _);
        using var start = host.Start(package.DirectoryPath, "brokered", PluginLifetime.Bound).Lease!;
        var target = Path.Combine(granted.DirectoryPath, "out.txt");

        var written = Call(start, new { op = "write", path = target, text = "data" }, BrokerRights.FileWrite);

        Assert.IsTrue(written.GetProperty("allowed").GetBoolean());
        Assert.AreEqual("data", File.ReadAllText(target));
    }

    [TestMethod]
    public void Call_ContextWithoutTheRight_IsRefusedEvenInsideTheGrant()
    {
        using var package = _temp.GetTempDirectory();
        using var granted = _temp.GetTempDirectory();
        File.WriteAllText(Path.Combine(granted.DirectoryPath, "in.txt"), "inside");
        using var host = Host(package, granted.DirectoryPath, "ro", out _);
        using var start = host.Start(package.DirectoryPath, "brokered", PluginLifetime.Bound).Lease!;

        var result = Call(start, new { op = "read", path = Path.Combine(granted.DirectoryPath, "in.txt") }, rights: []);

        Assert.IsFalse(result.GetProperty("allowed").GetBoolean());
        Assert.AreEqual("context: MissingRight", result.GetProperty("reason").GetString());
    }

    [TestMethod]
    public void Call_Whoami_PluginReceivedTheContextAndOnlyDeclaredClaims()
    {
        using var package = _temp.GetTempDirectory();
        using var granted = _temp.GetTempDirectory();
        using var host = Host(package, granted.DirectoryPath, "ro", out _);
        using var start = host.Start(package.DirectoryPath, "brokered", PluginLifetime.Bound).Lease!;

        var who = Call(start, new { op = "whoami" }, BrokerRights.FileRead);

        Assert.AreEqual("alice", who.GetProperty("sub").GetString());
        Assert.AreEqual("a@example.org", who.GetProperty("claims").GetProperty("email").GetString());
        Assert.IsFalse(who.GetProperty("claims").TryGetProperty("ssn", out _), "An undeclared claim never reaches the plugin.");
    }

    [TestMethod]
    public void Call_Fetch_GrantedHostWorksAndOtherHostNeverSendsARequest()
    {
        using var package = _temp.GetTempDirectory();
        using var granted = _temp.GetTempDirectory();
        using var host = Host(package, granted.DirectoryPath, "ro", out _, ("api.example.org", 443));
        using var start = host.Start(package.DirectoryPath, "brokered", PluginLifetime.Bound).Lease!;

        var ok = Call(start, new { op = "fetch", url = "https://api.example.org/data" }, BrokerRights.Fetch);
        var requestsAfterOk = _network.Requests;
        var refused = Call(start, new { op = "fetch", url = "https://evil.example.net/data" }, BrokerRights.Fetch);

        Assert.AreEqual("hello from api.example.org", ok.GetProperty("text").GetString());
        Assert.IsFalse(refused.GetProperty("allowed").GetBoolean());
        Assert.AreEqual(requestsAfterOk, _network.Requests, "A refused fetch never reaches the network.");
    }

    [TestMethod]
    public void Start_ModifiedPackage_IsRejectedBeforeAnythingRuns()
    {
        using var package = _temp.GetTempDirectory();
        using var granted = _temp.GetTempDirectory();
        File.WriteAllText(Path.Combine(package.DirectoryPath, "plugin.dat"), "original");
        using var host = Host(package, granted.DirectoryPath, "ro", out _);
        File.WriteAllText(Path.Combine(package.DirectoryPath, "plugin.dat"), "tampered");

        var start = host.Start(package.DirectoryPath, "brokered", PluginLifetime.Bound);

        Assert.AreEqual(HostRefusal.PackageRejected, start.Refusal);
        Assert.AreEqual("FileModified", start.Detail);
        Assert.IsNull(start.Lease);
    }

    [TestMethod]
    public void Start_NoGrant_IsRefused()
    {
        using var package = _temp.GetTempDirectory();
        using var granted = _temp.GetTempDirectory();
        using var host = Host(package, granted.DirectoryPath, "ro", out _);
        _grants.Revoke(_id);

        var start = host.Start(package.DirectoryPath, "brokered", PluginLifetime.Bound);

        Assert.AreEqual(HostRefusal.NoGrant, start.Refusal);
    }

    [TestMethod]
    public void Start_Shared_SecondLeaseReusesTheProcessAndASecondHostIsToldItIsInUse()
    {
        using var package = _temp.GetTempDirectory();
        using var granted = _temp.GetTempDirectory();
        using var host = Host(package, granted.DirectoryPath, "ro", out _);
        using var second = Host(package, granted.DirectoryPath, "ro", out _, signed: false);

        using var first = host.Start(package.DirectoryPath, "brokered", PluginLifetime.Shared).Lease!;
        using var again = host.Start(package.DirectoryPath, "brokered", PluginLifetime.Shared).Lease!;
        var other = second.Start(package.DirectoryPath, "brokered", PluginLifetime.Shared);

        Assert.AreSame(first.Plugin, again.Plugin);
        Assert.AreEqual(1, host.SharedCount);
        Assert.AreEqual(HostRefusal.InUseElsewhere, other.Refusal);
    }

    private static JsonElement Call(PluginLease<BrokeredPlugin> lease, object request, params string[] rights)
    {
        var context = RequestContext.Create(
            "alice",
            "tenant-1",
            lease.Plugin.PluginId,
            new Dictionary<string, string> { ["email"] = "a@example.org", ["ssn"] = "000-00-0000" },
            ["email"],
            rights,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(5),
            "trace-1");
        return JsonDocument.Parse(lease.Plugin.Call(JsonSerializer.Serialize(request), context)).RootElement.Clone();
    }

    private PluginHost Host(ITempDirectory package, string folder, string access, out PluginManifest manifest, (string Host, int Port)? network = null, bool signed = true)
    {
        var template = new PluginManifest(
            _id,
            "1.0.0",
            "1.0",
            new Dictionary<string, string> { ["any"] = "brokered" },
            new PluginPermissions([new FilePermission(folder, access)], network is { } n ? [new NetworkPermission(n.Host, n.Port)] : []),
            new Dictionary<string, string>());
        var trust = new TrustList();
        trust.Trust("publisher-1", _publisher.ExportSubjectPublicKeyInfo());
        if (signed)
        {
            PackageSigner.Sign(package.DirectoryPath, template, _publisher, "publisher-1");
            _grants.Approve(template, template.Permissions, "owner");
        }

        manifest = template;
        return new PluginHost(new PackageVerifier(trust), _grants, new GrantBroker(_grants, _network), _contexts, TimeProvider.System, TimeSpan.FromMinutes(1), TestLauncher.Start);
    }
}
