using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>REQ-012: the immutable request context and the broker that checks the id a plugin presents.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class RequestContextTests
{
    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static readonly DateTimeOffset _start = new(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);

    private static RequestContext Make(string plugin = "reports", string sub = "u-1", string[]? rights = null) => RequestContext.Create(
        sub,
        "t-1",
        plugin,
        new Dictionary<string, string> { ["email"] = "a@example.test", ["name"] = "Ada", ["token"] = "secret-jwt" },
        ["email", "name"],
        rights ?? ["reports.read", "reports.export"],
        _start,
        TimeSpan.FromMinutes(5),
        "trace-1");

    [TestMethod]
    public void Create_UndeclaredClaim_IsDroppedAndTheTokenNeverAppears()
    {
        var context = Make();

        Assert.HasCount(2, context.Claims);
        Assert.IsFalse(context.ToJson().Contains("secret-jwt", StringComparison.Ordinal));
        Assert.IsFalse(context.Canonical.Contains("token", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Id_SameInputs_AreIdenticalRegardlessOfInputOrder()
    {
        var first = Make(rights: ["b", "a"]);
        var second = Make(rights: ["a", "b", "a"]);

        Assert.AreEqual(first.Id, second.Id);
        Assert.AreEqual(64, first.Id.Length);
    }

    [TestMethod]
    [DataRow("sub")]
    [DataRow("plugin")]
    [DataRow("rights")]
    public void Id_AnyFieldChange_ChangesTheId(string changed)
    {
        var baseline = Make();
        var other = changed switch
        {
            "sub" => Make(sub: "u-2"),
            "plugin" => Make(plugin: "other"),
            _ => Make(rights: ["reports.read"]),
        };

        Assert.AreNotEqual(baseline.Id, other.Id);
    }

    [TestMethod]
    public void ToJson_IsValidJsonCarryingTheIdAndSortedKeys()
    {
        var context = Make();

        using var document = JsonDocument.Parse(context.ToJson());

        Assert.AreEqual(context.Id, document.RootElement.GetProperty("id").GetString());
        Assert.AreEqual("u-1", document.RootElement.GetProperty("sub").GetString());
        Assert.StartsWith("{\"aud\":", context.Canonical, StringComparison.Ordinal);
    }

    [TestMethod]
    public void Create_NonPositiveLifetime_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => RequestContext.Create("u", "t", "p", [], [], [], _start, TimeSpan.Zero, "x"));
    }

    [TestMethod]
    public void Check_Rules_UnknownWrongPluginExpiredMissingRightAndAllowed()
    {
        var time = new FixedTime(_start);
        var broker = new ContextBroker(time);
        var context = broker.Issue(Make());

        Assert.AreEqual(ContextRefusal.None, broker.Check("reports", context.Id, "reports.read"));
        Assert.AreEqual(ContextRefusal.Unknown, broker.Check("reports", new string('0', 64)));
        Assert.AreEqual(ContextRefusal.WrongPlugin, broker.Check("other", context.Id));
        Assert.AreEqual(ContextRefusal.MissingRight, broker.Check("reports", context.Id, "reports.delete"));
        time.Now = _start.AddMinutes(5);
        Assert.AreEqual(ContextRefusal.Expired, broker.Check("reports", context.Id));
    }

    [TestMethod]
    public void Check_ForgedContextThePluginEdited_IsUnknownToTheBroker()
    {
        var broker = new ContextBroker(new FixedTime(_start));
        broker.Issue(Make(rights: ["reports.read"]));
        var forged = Make(rights: ["reports.read", "admin"]);

        Assert.AreEqual(ContextRefusal.Unknown, broker.Check("reports", forged.Id, "admin"));
    }

    [TestMethod]
    public void Release_AfterTheRequest_RefusesTheId()
    {
        var broker = new ContextBroker(new FixedTime(_start));
        var context = broker.Issue(Make());

        broker.Release(context.Id);

        Assert.AreEqual(ContextRefusal.Unknown, broker.Check("reports", context.Id));
    }
}
