using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.Configuration;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>The real TUI screenshot for docs/applications/devterm/user-guide/routing.md: <see cref="RoutingMode"/> with a connected broker, two rules and some traffic.</summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class RoutingScreenshotTests
{
    [TestMethod]
    public async Task Routing_WithTraffic_IsCaptured()
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
        await tab.Session.OpenAsync();
        for (var i = 0; i < 100 && tab.Routing.State != RoutingState.Connected; i++)
        {
            await Task.Delay(10);
        }

        var router = tab.Routing.Router!;
        foreach (var line in new[] { "T=21", "T=22", "hello" })
        {
            router.Render(new global::System.Buffers.ReadOnlySequence<byte>(global::System.Text.Encoding.UTF8.GetBytes(line + "\r\n")));
        }

        var dump = "";
        TuiTestRunner.RunHeadlessApp(app =>
        {
            var parts = RoutingMode.BuildWindow(app, new RoutingViewModel(tab));
            var token = app.Begin(parts.Window) ?? throw new NotSupportedException();
            app.LayoutAndDraw(true);
            try
            {
                parts.Sample.Text = "T=21";
                parts.RunTest();
                app.LayoutAndDraw(true);
                dump = TuiTestRunner.DumpBuffer();
                var images = TuiReview.DocImages;
                Directory.CreateDirectory(images);
                TuiScreenshot.Save(Path.Combine(images, "tui-routing.png"));
            }
            finally
            {
                app.End(token);
                parts.Window.Dispose();
            }
        });

        File.WriteAllText(Path.Combine(TuiReview.DocImages, "tui-routing.txt"), global::System.Text.RegularExpressions.Regex.Replace(dump, @"\d\d:\d\d:\d\d", "12:00:00"));
        await tab.Session.CloseAsync();

        Assert.Contains("Broker: Connected", dump);
        Assert.Contains("Unmatched: 1", dump);
        Assert.Contains("bench/temp", dump);
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
