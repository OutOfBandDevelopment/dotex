using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Utilities;
using OoBDev.TestUtilities;
using System.Threading.Tasks;

namespace OoBDev.CapabilityName.Tests;

[TestClass]
public class CapabilityNameManagerTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RunAsync_DelegatesToSelectedProvider_ReturnsProviderResult()
    {
        // Stage
        var provider = new Mock<ICapabilityNameProvider>(MockBehavior.Strict);
        var selected = new Mock<ISelectedService<ICapabilityNameProvider>>(MockBehavior.Strict);
        selected.SetupGet(s => s.Value).Returns(provider.Object);
        provider.Setup(p => p.ExecuteAsync("in")).ReturnsAsync("out");

        // Test
        var result = await new CapabilityNameManager(selected.Object).RunAsync("in");

        // Assert
        Assert.AreEqual("out", result);

        // Verify
        provider.VerifyAll();
        selected.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddCapabilityNameServices_Registers_Manager()
    {
        // Stage
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .TryAddCapabilityNameServices(configuration, new CapabilityNameBuilder());

        // Test: a provider is registered by an adapter, so resolve with a stub one
        using var sp = services
            .AddSingleton(new Mock<ICapabilityNameProvider>().Object)
            .BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<CapabilityNameManager>(sp.GetRequiredService<ICapabilityNameManager>());
    }
}
