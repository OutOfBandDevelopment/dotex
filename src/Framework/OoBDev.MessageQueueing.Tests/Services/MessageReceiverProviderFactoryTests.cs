using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace OoBDev.MessageQueueing.Tests.Services;

[TestClass]
public class MessageReceiverProviderFactoryTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateTest()
    {
        var config = new ConfigurationBuilder()
            .Build();

        var safeProvider = (
            providerKey: "providerKey",
            simpleTargetName: "simpleTargetName",
            simpleMessageName: "simpleMessageName",
            configPath: "configPath"
            );

        var safeConfig = (
            configurationSection: config.GetSection("test"),
            simpleTargetName: "simpleTargetName",
            simpleMessageName: "simpleMessageName",
            configPath: "configPath"
            );

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockHandler = mockRepo.Create<IMessageQueueHandler>();
        var mockPropertyResolver = mockRepo.Create<IMessagePropertyResolver>();
        var mockReceiverProvider = mockRepo.Create<IMessageReceiverProvider>();
        var mockHandlerProvider = mockRepo.Create<IMessageHandlerProvider>();

        var services = new ServiceCollection();
        services.TryAddKeyedTransient(safeProvider.providerKey, (_, _) => mockReceiverProvider.Object);
        services.TryAddTransient(_ => mockHandlerProvider.Object);

        var serviceProvider = services.BuildServiceProvider();

        mockPropertyResolver.Setup(s => s.ProviderSafe(It.IsAny<Type>(), It.IsAny<Type>())).Returns(safeProvider);
        mockPropertyResolver.Setup(s => s.ConfigurationSafe(It.IsAny<Type>(), It.IsAny<Type>())).Returns(safeConfig);

        mockReceiverProvider.Setup(s => s.SetHandlerProvider(mockHandlerProvider.Object)).Returns(mockReceiverProvider.Object);

        var factory = new MessageReceiverProviderFactory(
            [mockHandler.Object],
            mockPropertyResolver.Object,
            serviceProvider,
            TestLogger.CreateLogger<MessageReceiverProviderFactory>()
            );

        var providers = factory.Create().ToArray();

        Assert.IsNotNull(providers);
        Assert.AreEqual(1, providers.Length);

        mockRepo.VerifyAll();
    }
}
