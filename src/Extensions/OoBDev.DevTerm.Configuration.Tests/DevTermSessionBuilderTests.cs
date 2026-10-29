using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;

using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Configuration.Discovery;
namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class DevTermSessionBuilderTests
{
    [TestMethod]
    public void Build_WithValidOptions_ReturnsAnUnopenedSessionAndTheNamedPresenter()
    {
        var result = DevTermSessionBuilder.Build(new CliOptions { Transport = "loopback", Presenter = ["hex"] });

        Assert.IsNotNull(result.Session);
        Assert.AreSequenceEqual(["hex"], [.. result.Session.Presenters.Select(p => p.Name)]);
        Assert.AreEqual(ConnectionState.Closed, result.Session.State);
    }

    [TestMethod]
    public void Build_WithUnknownPresenter_Throws()
    {
        var options = new CliOptions { Transport = "loopback", Presenter = ["not-a-real-presenter"] };

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => DevTermSessionBuilder.Build(options));
        Assert.Contains("not-a-real-presenter", ex.Message);
    }

    [TestMethod]
    public void Build_WithSeveralPresenters_FansOutToAllOfThemInOrder()
    {
        var result = DevTermSessionBuilder.Build(new CliOptions { Transport = "loopback", Presenter = ["ascii", "hex", "ASCII"] });

        Assert.AreSequenceEqual(["ascii", "hex"], [.. result.Session.Presenters.Select(p => p.Name)], "Duplicates (any case) collapse.");
    }

    [TestMethod]
    public void Build_WithAParserThatIsNotARegisteredPresenter_Throws()
    {
        var options = new CliOptions { Transport = "loopback", Parser = "nope" };

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => DevTermSessionBuilder.Build(options));
        Assert.Contains("nope", ex.Message);
    }

    [TestMethod]
    public void Build_ParserDefaultsToTheFirstPresenter_AndIsResolvableFromTheCatalog()
    {
        var result = DevTermSessionBuilder.Build(new CliOptions { Transport = "loopback", Presenter = ["decimal", "hex"] });

        Assert.IsTrue(result.Catalog.TryGetInput("decimal", out _));
        Assert.AreSequenceEqual(["ascii", "utf8", "hex", "decimal", "octal", "binary"], [.. result.Catalog.InputNames], Microsoft.VisualStudio.TestTools.UnitTesting.SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public async Task Build_TwiceWithDifferentOptions_ProducesIndependentSessions()
    {
        var first = DevTermSessionBuilder.Build(new CliOptions { Transport = "loopback", Presenter = ["ascii"] });
        var second = DevTermSessionBuilder.Build(new CliOptions { Transport = "loopback", Presenter = ["hex"] });

        Assert.AreNotSame(first.Session, second.Session);
        Assert.AreNotEqual(first.Session.Presenters[0].Name, second.Session.Presenters[0].Name);

        await first.Session.DisposeAsync();
        await second.Session.DisposeAsync();
    }
}
