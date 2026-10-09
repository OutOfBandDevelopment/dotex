using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.CapabilityName;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.VendorName.CapabilityName.Tests;

[TestClass]
public class VendorNameCapabilityNameProviderTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ExecuteAsync_DelegatesToFactory_ReturnsResponse()
    {
        // Stage
        var factory = new Mock<IVendorNameClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.SendAsync("in")).ReturnsAsync("out");

        // Test
        var result = await new VendorNameCapabilityNameProvider(factory.Object).ExecuteAsync("in");

        // Assert
        Assert.AreEqual("out", result);

        // Verify
        factory.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddServices_WithoutConfiguration_RegistersNothing()
    {
        // Stage
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        // Test
        var services = new ServiceCollection().TryAddVendorNameCapabilityNameServices(configuration);

        // Assert
        Assert.AreEqual(0, services.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddServices_WithAccountId_RegistersDefaultAndKeyedProvider()
    {
        // Stage
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["VendorNameOptions:AccountId"] = "abc" })
            .Build();

        // Test
        using var sp = new ServiceCollection()
            .TryAddVendorNameCapabilityNameServices(configuration)
            .BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<VendorNameCapabilityNameProvider>(sp.GetRequiredService<ICapabilityNameProvider>());
        Assert.IsInstanceOfType<VendorNameCapabilityNameProvider>(
            sp.GetRequiredKeyedService<ICapabilityNameProvider>(VendorNameGlobals.ProviderKey));
    }
}
