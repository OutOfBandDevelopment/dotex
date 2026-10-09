using OoBDev.Documents.Containers;
using OoBDev.Documents.Tests.TestTargets;
using OoBDev.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OoBDev.Documents.Tests.Containers;

[TestClass]
public class ServiceRegistryTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_IBlobContainer__ContainerTargetClass_Test()
    {
        var provider = new Mock<IBlobContainerProvider>(MockBehavior.Loose);
        var services = new ServiceCollection()
            .AddLogging()
            .AddTransient(_ => provider.Object)
            .TryAddDocumentServices()
            .BuildServiceProvider();

        var wrapper = services.GetRequiredService<IBlobContainer<ContainerTargetClass>>();
        Assert.IsNotNull(wrapper);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_IBlobContainer__Keyed_Test()
    {
        var provider = new Mock<IBlobContainerProvider>(MockBehavior.Loose);
        var services = new ServiceCollection()
            .AddLogging()
            .AddKeyedTransient<IBlobContainer>("test-name", (_, _) => provider.Object)
            .TryAddDocumentServices()
            .BuildServiceProvider();

        var wrapper = services.GetKeyedService<IBlobContainer>("test-name");
        Assert.IsNotNull(wrapper);
    }
}
