using OoBDev.Documents.Tests.TestTargets;
using OoBDev.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Documents.Tests.Containers;

[TestClass]
public class ServiceRegistryTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public void Create_IBlobContainer__ContainerTargetClass_Test()
    {
        var services = new ServiceCollection()
            .TryAddDocumentServices()
            .BuildServiceProvider();

        var wrapper = services.GetRequiredService<IBlobContainer<ContainerTargetClass>>();
        Assert.IsNotNull(wrapper);
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public void Create_IBlobContainer__Keyed_Test()
    {
        var services = new ServiceCollection()
            .TryAddDocumentServices()
            .BuildServiceProvider();

        var wrapper = services.GetKeyedService<IBlobContainer>("TestName");
        Assert.IsNotNull(wrapper);
    }
}
