using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Sandbox.Tests;

/// <summary>The grant store and broker (REQ-002, REQ-003, REQ-004, REQ-007): approved subset only, refusals logged, exact hosts.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class GrantBrokerTests
{
    private sealed class Stub(Func<Uri, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            return Task.FromResult(respond(request.RequestUri!));
        }
    }

    private static readonly string _data = global::System.IO.Path.GetFullPath("grant-test-data");
    private static readonly string _other = global::System.IO.Path.GetFullPath("grant-test-other");

    private static readonly PluginManifest _manifest = new(
        "telemetry",
        "1.0.0",
        "1.0",
        new Dictionary<string, string>(),
        new PluginPermissions([new FilePermission(_data, "rw"), new FilePermission(_other, "ro")], [new NetworkPermission("api.example.org", 443), new NetworkPermission("other.example.org", 443)]),
        new Dictionary<string, string>());

    private static GrantStore Store(PluginPermissions approved)
    {
        var store = new GrantStore(TimeProvider.System);
        store.Approve(_manifest, approved, "alice");
        return store;
    }

    [TestMethod]
    public void Approve_MoreThanRequested_IsRefused()
    {
        var store = new GrantStore(TimeProvider.System);

        Assert.ThrowsExactly<ArgumentException>(() => store.Approve(_manifest, new PluginPermissions(Network: [new NetworkPermission("evil.example.org", 443)]), "alice"));
        Assert.ThrowsExactly<ArgumentException>(() => store.Approve(_manifest, new PluginPermissions([new FilePermission(_other, "rw")]), "alice"));
    }

    [TestMethod]
    public void Approve_Subset_IsStoredWithApproverAndAudited()
    {
        var store = Store(new PluginPermissions([new FilePermission(_data, "ro")], [new NetworkPermission("api.example.org", 443)]));

        var grant = store.Find("telemetry")!;

        Assert.AreEqual("alice", grant.ApprovedBy);
        Assert.HasCount(1, grant.Files);
        Assert.AreEqual("approve", store.Audit.Single().Action);
    }

    [TestMethod]
    public void Hash_SamePermissionsInAnyOrder_AreEqualAndDifferWhenWider()
    {
        var a = new Grant("p", [new FilePermission("x", "ro")], [new NetworkPermission("a", 1), new NetworkPermission("b", 2)], "me", default);
        var b = new Grant("p", [new FilePermission("x", "ro")], [new NetworkPermission("b", 2), new NetworkPermission("a", 1)], "you", default);
        var wider = new Grant("p", [new FilePermission("x", "rw")], [new NetworkPermission("a", 1), new NetworkPermission("b", 2)], "me", default);

        Assert.AreEqual(a.Hash, b.Hash);
        Assert.AreNotEqual(a.Hash, wider.Hash);
    }

    [TestMethod]
    public void CheckFile_NoGrant_IsRefusedAndLogged()
    {
        var store = new GrantStore(TimeProvider.System);

        var decision = new GrantBroker(store).CheckFile("telemetry", _data, write: false);

        Assert.IsFalse(decision.Allowed);
        Assert.IsFalse(store.Audit.Single().Allowed);
    }

    [TestMethod]
    public void CheckFile_InsideGrantedFolder_IsAllowed()
    {
        var broker = new GrantBroker(Store(new PluginPermissions([new FilePermission(_data, "rw")])));

        Assert.IsTrue(broker.CheckFile("telemetry", global::System.IO.Path.Combine(_data, "sub", "a.txt"), write: true).Allowed);
        Assert.IsTrue(broker.CheckFile("telemetry", _data, write: false).Allowed);
    }

    [TestMethod]
    public void CheckFile_DotDotEscapeAndSiblingPrefix_AreRefused()
    {
        var broker = new GrantBroker(Store(new PluginPermissions([new FilePermission(_data, "rw")])));

        Assert.IsFalse(broker.CheckFile("telemetry", global::System.IO.Path.Combine(_data, "..", "secret.txt"), write: false).Allowed, "dot-dot leaves the folder");
        Assert.IsFalse(broker.CheckFile("telemetry", _data + "-evil", write: false).Allowed, "a sibling folder sharing the prefix is outside");
    }

    [TestMethod]
    public void CheckFile_WriteToReadOnlyGrant_IsRefused()
    {
        var broker = new GrantBroker(Store(new PluginPermissions([new FilePermission(_other, "ro")])));

        Assert.IsTrue(broker.CheckFile("telemetry", global::System.IO.Path.Combine(_other, "a.txt"), write: false).Allowed);
        Assert.IsFalse(broker.CheckFile("telemetry", global::System.IO.Path.Combine(_other, "a.txt"), write: true).Allowed);
    }

    [TestMethod]
    public void CheckNetwork_ExactHostAndPort_IsAllowedOthersRefused()
    {
        var broker = new GrantBroker(Store(new PluginPermissions(Network: [new NetworkPermission("api.example.org", 443)])));

        Assert.IsTrue(broker.CheckNetwork("telemetry", "API.example.org", 443).Allowed);
        Assert.IsFalse(broker.CheckNetwork("telemetry", "api.example.org", 80).Allowed, "other port");
        Assert.IsFalse(broker.CheckNetwork("telemetry", "evil.api.example.org", 443).Allowed, "no wildcard or suffix match");
        Assert.IsFalse(broker.CheckNetwork("telemetry", "127.0.0.1", 443).Allowed);
    }

    [TestMethod]
    public void CheckNetwork_AfterRevoke_IsRefused()
    {
        var store = Store(new PluginPermissions(Network: [new NetworkPermission("api.example.org", 443)]));
        store.Revoke("telemetry");

        Assert.IsFalse(new GrantBroker(store).CheckNetwork("telemetry", "api.example.org", 443).Allowed);
    }

    [TestMethod]
    public async Task Fetch_GrantedHost_ReturnsTheBody()
    {
        var stub = new Stub(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("hello", Encoding.UTF8) });
        var broker = new GrantBroker(Store(new PluginPermissions(Network: [new NetworkPermission("api.example.org", 443)])), stub);

        var result = await broker.FetchAsync("telemetry", new Uri("https://api.example.org/x"));

        Assert.IsTrue(result.Decision.Allowed);
        Assert.AreEqual(200, result.Status);
        Assert.AreEqual("hello", Encoding.UTF8.GetString(result.Body));
    }

    [TestMethod]
    public async Task Fetch_UngrantedHost_NeverReachesTheNetwork()
    {
        var stub = new Stub(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var store = Store(new PluginPermissions(Network: [new NetworkPermission("api.example.org", 443)]));
        var broker = new GrantBroker(store, stub);

        var result = await broker.FetchAsync("telemetry", new Uri("https://evil.example.org/x"));

        Assert.IsFalse(result.Decision.Allowed);
        Assert.IsEmpty(stub.Requests, "A refused call sends nothing.");
        Assert.IsTrue(store.Audit.Any(a => !a.Allowed && a.Target.StartsWith("evil.example.org", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task Fetch_RedirectToUngrantedHost_IsStopped()
    {
        var stub = new Stub(uri => uri.Host == "api.example.org"
            ? new HttpResponseMessage(HttpStatusCode.Found) { Headers = { Location = new Uri("https://evil.example.org/steal") } }
            : new HttpResponseMessage(HttpStatusCode.OK));
        var broker = new GrantBroker(Store(new PluginPermissions(Network: [new NetworkPermission("api.example.org", 443)])), stub);

        var result = await broker.FetchAsync("telemetry", new Uri("https://api.example.org/x"));

        Assert.IsFalse(result.Decision.Allowed);
        Assert.HasCount(1, stub.Requests);
    }

    [TestMethod]
    public async Task Fetch_RedirectToAnotherGrantedHost_IsFollowed()
    {
        var stub = new Stub(uri => uri.Host == "api.example.org"
            ? new HttpResponseMessage(HttpStatusCode.Found) { Headers = { Location = new Uri("https://other.example.org/y") } }
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
        var broker = new GrantBroker(Store(new PluginPermissions(Network: [new NetworkPermission("api.example.org", 443), new NetworkPermission("other.example.org", 443)])), stub);

        var result = await broker.FetchAsync("telemetry", new Uri("https://api.example.org/x"));

        Assert.IsTrue(result.Decision.Allowed);
        Assert.HasCount(2, stub.Requests);
    }

    [TestMethod]
    public async Task Fetch_RedirectLoop_IsStopped()
    {
        var stub = new Stub(uri => new HttpResponseMessage(HttpStatusCode.Found) { Headers = { Location = uri } });
        var broker = new GrantBroker(Store(new PluginPermissions(Network: [new NetworkPermission("api.example.org", 443)])), stub);

        var result = await broker.FetchAsync("telemetry", new Uri("https://api.example.org/x"));

        Assert.IsFalse(result.Decision.Allowed);
        Assert.AreEqual("too many redirects", result.Decision.Reason);
    }
}
