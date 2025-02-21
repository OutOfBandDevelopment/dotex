using OoBDev.Documents.Containers;
using OoBDev.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.ComponentModel.DataAnnotations.Schema;

namespace OoBDev.Documents.Tests.Containers;

[TestClass]
public class BlobContainerFactoryTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateTest_Named_Keyed()
    {
        var testName = "container target";

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockProvider = mockRepo.Create<IBlobContainerProvider>(MockBehavior.Loose);

        var services = new ServiceCollection();
        services.AddKeyedTransient(testName, (_, _) => mockProvider.Object);
        var serviceProvider = services.BuildServiceProvider();

        var factory = new BlobContainerFactory(serviceProvider, [], TestLogger.CreateLogger<BlobContainerFactory>());

        var instance = factory.Create(testName);

        Assert.IsNotNull(instance);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateTest_Named_Factory()
    {
        var testName = "container target";

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockProvider = mockRepo.Create<IBlobContainerProvider>(MockBehavior.Loose);
        var mockFactory = mockRepo.Create<IBlobContainerProviderFactory>(MockBehavior.Loose);
        mockFactory.Setup(s => s.Create(testName)).Returns(mockProvider.Object);

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var factory = new BlobContainerFactory(serviceProvider, [mockFactory.Object], TestLogger.CreateLogger<BlobContainerFactory>());

        var instance = factory.Create(testName);

        Assert.IsNotNull(instance);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateTest_NoTag()
    {
        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockProvider = mockRepo.Create<IBlobContainerProvider>(MockBehavior.Loose);

        var services = new ServiceCollection();
        services.AddTransient(_ => mockProvider.Object);
        var serviceProvider = services.BuildServiceProvider();

        var factory = new BlobContainerFactory(serviceProvider, [], TestLogger.CreateLogger<BlobContainerFactory>());

        var instance = factory.Create<NoTag>();
        Assert.IsNotNull(instance);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateTest_TableTag()
    {
        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockProvider = mockRepo.Create<IBlobContainerProvider>(MockBehavior.Loose);

        var services = new ServiceCollection();
        services.AddTransient(_ => mockProvider.Object);
        var serviceProvider = services.BuildServiceProvider();

        var factory = new BlobContainerFactory(serviceProvider, [], TestLogger.CreateLogger<BlobContainerFactory>());

        var instance = factory.Create<TableTag>();
        Assert.IsNotNull(instance);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateTest_ContainerTag()
    {
        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockProvider = mockRepo.Create<IBlobContainerProvider>(MockBehavior.Loose);

        var services = new ServiceCollection();
        services.AddTransient(_ => mockProvider.Object);
        var serviceProvider = services.BuildServiceProvider();

        var factory = new BlobContainerFactory(serviceProvider, [], TestLogger.CreateLogger<BlobContainerFactory>());

        var instance = factory.Create<ContainerTag>();
        Assert.IsNotNull(instance);

        mockRepo.VerifyAll();
    }

    public const string TableName = "table";
    public const string ContainerName = "container";
    public class NoTag { }
    [Table(TableName)]
    public class TableTag { }
    [BlobContainer(ContainerName = ContainerName)]
    public class ContainerTag { }

    /*
    public virtual IBlobContainer<T> Create<T>() =>
        new WrappedBlobContainer<T>(
        Create(
            typeof(T).GetCustomAttribute<BlobContainerAttribute>()?.ContainerName ??
            typeof(T).GetCustomAttribute<TableAttribute>()?.Name ??
            typeof(T).Name
            ));
    */
}
