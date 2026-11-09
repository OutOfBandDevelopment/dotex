using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The routing window with a connected broker, two rules and some traffic, captured for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class RoutingScreenshotTests
{
    [TestMethod]
    public void RoutingWindow_WithTraffic_IsCaptured() =>
        WpfScreenshot.Capture(BuildWindow, "wpf-routing.png", 760, 760);

    internal static RoutingWindow BuildWindow()
    {
        var options = new CliOptions { Transport = "loopback" };
        options.Routing.Host = "localhost";
        options.Routing.Rules =
        [
            new RoutingRule { Direction = RoutingDirection.DeviceToBroker, Match = @"^T=(?<t>\d+)$", Topic = "bench/temp", Payload = "${t}" },
            new RoutingRule { Direction = RoutingDirection.BrokerToDevice, Match = "(?<payload>.*)", Topic = "bench/cmd", Send = "SET ${payload}", Confirm = true },
        ];
        var tab = SessionTab.Build(options);
        tab.RoutingLinkFactory = new FakeLinkFactory();
        tab.Session.OpenAsync().GetAwaiter().GetResult();
        for (var i = 0; i < 200 && tab.Routing.State != RoutingState.Connected; i++)
        {
            Thread.Sleep(10);
        }

        Assert.AreEqual(RoutingState.Connected, tab.Routing.State);
        foreach (var line in new[] { "T=21", "T=22", "hello" })
        {
            tab.Routing.Router!.Render(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(line + "\r\n")));
        }

        var dialog = new RoutingDialogViewModel(new RoutingViewModel(tab)) { SampleText = "T=21" };
        dialog.TestRulesCommand.Execute(null);
        return new RoutingWindow(dialog);
    }

    private sealed class FakeLinkFactory : IRoutingLinkFactory
    {
        public IRoutingLink Create(RoutingOptions options) => new FakeLink();
    }

    private sealed class FakeLink : IRoutingLink
    {
        public event Action<Exception?>? Lost
        {
            add { }
            remove { }
        }

        public Task StartAsync(MessageRouter router, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task PublishAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
