using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.DevTerm.Panels;
using OoBDev.UiDefinitions;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
using System;
using System.Buffers;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.DevTerm.Core.Tests.Panels;

[TestClass]
public sealed class DevicePanelRegistrationTests
{
    private sealed class TestDecoder : IDeviceDecoder
    {
        public string Name => "test-panel";

        public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged;

        public IReadOnlyList<string> Decode(ReadOnlySequence<byte> data)
        {
            ValuesChanged?.Invoke(this, new Dictionary<string, string> { ["length"] = data.Length.ToString(CultureInfo.InvariantCulture) });
            return [$"got {data.Length}"];
        }
    }

    private sealed class TestPanel : IDevicePanel
    {
        public string Id => "test-panel";

        public string MenuTitle => "_Test";

        public string PresenterName => "test-panel";

        public bool IsAvailable(string transport, int vendorId, int productId) => vendorId == 1;

        public UiDefinition BuildDefinition() => new() { Name = "Test" };

        public IDeviceDecoder? SeenDecoder { get; private set; }

        public IControlSurface CreateSurface(DeviceSend send, IDeviceDecoder decoder)
        {
            SeenDecoder = decoder;
            return new SendingSurface(send);
        }

        public IDeviceDecoder CreateDecoder() => new TestDecoder();
    }

    private sealed class SendingSurface(DeviceSend send) : IControlSurface
    {
        public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default) =>
            send(new byte[] { 7 }, cancellationToken);
    }

    [TestMethod, TestCategory("Unit")]
    public void TryAddDevicePanel_Registered_ProvidesContributionAndPresenter()
    {
        // Stage
        var services = new ServiceCollection().TryAddDevicePanel<TestPanel>();
        using var provider = services.BuildServiceProvider();

        // Test
        var contribution = provider.GetServices<IDevicePanelContribution>().Single();
        var presenter = provider.GetServices<IPresenter>().Single();

        // Assert
        Assert.AreEqual("test-panel", contribution.Id);
        Assert.AreEqual("_Test", contribution.MenuTitle);
        Assert.AreEqual("test-panel", contribution.PresenterName);
        Assert.IsTrue(contribution.IsAvailable("hid", 1, 0));
        Assert.IsFalse(contribution.IsAvailable("hid", 2, 0));
        Assert.AreEqual("Test", contribution.BuildDefinition().Name);
        Assert.AreEqual("test-panel", presenter.Name);
    }

    [TestMethod, TestCategory("Unit")]
    public void TryAddDevicePanel_CalledTwice_RegistersOnce()
    {
        var services = new ServiceCollection().TryAddDevicePanel<TestPanel>().TryAddDevicePanel<TestPanel>();
        using var provider = services.BuildServiceProvider();

        Assert.HasCount(1, provider.GetServices<IDevicePanelContribution>());
        Assert.HasCount(1, provider.GetServices<IPresenter>());
    }

    [TestMethod, TestCategory("Unit")]
    public void Presenter_Resolved_GetsAFreshDecoderEachTime()
    {
        using var provider = new ServiceCollection().TryAddDevicePanel<TestPanel>().BuildServiceProvider();

        var first = provider.GetServices<IPresenter>().Single();
        var second = provider.GetServices<IPresenter>().Single();

        Assert.AreNotSame(first, second);
    }

    [TestMethod, TestCategory("Unit")]
    public void Presenter_Render_DecodesAndForwardsValues()
    {
        using var provider = new ServiceCollection().TryAddDevicePanel<TestPanel>().BuildServiceProvider();
        var presenter = provider.GetServices<IPresenter>().Single();
        IReadOnlyDictionary<string, string>? values = null;
        ((IStructuredPresenter)presenter).ValuesChanged += (_, v) => values = v;

        var lines = presenter.Render(new ReadOnlySequence<byte>(new byte[] { 1, 2, 3 }));

        CollectionAssert.AreEqual(new[] { "got 3" }, lines.ToArray());
        Assert.AreEqual("3", values!["length"]);
    }

    [TestMethod, TestCategory("Unit")]
    public async Task CreateSurface_Invoke_SendsThroughTheSession()
    {
        // Stage
        var written = new List<byte[]>();
        var adapter = new Mock<IDeviceAdapter>();
        adapter.SetupGet(t => t.Input).Returns(new Pipe().Reader);
        adapter.SetupGet(t => t.Output).Returns(DeviceAdapterOutput.Create((data, _) =>
        {
            written.Add(data.ToArray());
            return Task.CompletedTask;
        }));
        adapter.SetupGet(t => t.State).Returns(ConnectionState.Open);
        var session = new Session(adapter.Object, new Pipeline([]));

        // Test
        var surface = new DevicePanelContribution(new TestPanel()).CreateSurface(session);
        await surface.InvokeAsync("x", null, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(7, written.Single()[0]);
    }

    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory("Unit")]
    public void CreateSurface_GivenTheSessionPresenter_PassesItsDecoder()
    {
        var panel = new TestPanel();
        var decoder = new TestDecoder();
        var adapter = new Mock<IDeviceAdapter>();
        var session = new Session(adapter.Object, new Pipeline([]));

        new DevicePanelContribution(panel).CreateSurface(session, new DeviceDecoderPresenter(decoder));

        Assert.AreSame(decoder, panel.SeenDecoder);
    }

    [TestMethod, TestCategory("Unit")]
    public void CreateSurface_WithoutAPresenter_PassesAFreshDecoder()
    {
        var panel = new TestPanel();
        var adapter = new Mock<IDeviceAdapter>();
        var session = new Session(adapter.Object, new Pipeline([]));

        new DevicePanelContribution(panel).CreateSurface(session);

        Assert.IsInstanceOfType<TestDecoder>(panel.SeenDecoder);
    }
}
