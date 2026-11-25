using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Remote;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class RemoteTransportWiringTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task AddDevTermFrontEnd_RemoteTransport_ResolvesTheRemoteAdapterWithItsOptions()
    {
        // Stage
        var options = new CliOptions { Transport = "remote", Host = "https://bench:5000", RemoteConnection = "scope", Password = "key" };
        var services = new ServiceCollection();
        services.AddLogging();

        // Test
        services.AddDevTermFrontEnd(options);
        await using var provider = services.BuildServiceProvider();
        var adapter = provider.GetRequiredService<IDeviceAdapter>();
        TestContext.WriteLine($"adapter={adapter.GetType().Name} type={adapter.Type}");

        // Assert
        Assert.IsInstanceOfType<RemoteDeviceAdapter>(adapter);
        Assert.AreEqual("remote", adapter.Type);
    }

    [TestMethod]
    public void Describe_RemoteTransport_NamesTheConnectionAndHost()
    {
        // Stage
        var options = new CliOptions { Transport = "remote", Host = "https://bench:5000", RemoteConnection = "scope" };

        // Test
        var text = ConnectionDescription.Definition(options);
        TestContext.WriteLine(text);

        // Assert
        StringAssert.Contains(text, "scope");
        StringAssert.Contains(text, "bench");
    }
}
