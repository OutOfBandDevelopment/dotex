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

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_WithMultipleProviders_ReturnsAllProviders()
    {
        // This test verifies the factory can discover and instantiate multiple receiver providers
        // (e.g., both in-process and external providers like RabbitMQ/SQS)

        var config = new ConfigurationBuilder()
            .Build();

        var rabbitMQProvider = (
            providerKey: "rabbitmq",
            simpleTargetName: "simpleTargetName",
            simpleMessageName: "simpleMessageName",
            configPath: "MessageQueue:RabbitMQ:Config"
            );

        var inProcessProvider = (
            providerKey: "in-process",
            simpleTargetName: "simpleTargetName2",
            simpleMessageName: "simpleMessageName2",
            configPath: "MessageQueue:InProcess:Config"
            );

        var rabbitMQConfig = (
            configurationSection: config.GetSection("MessageQueue:RabbitMQ:Config"),
            simpleTargetName: "simpleTargetName",
            simpleMessageName: "simpleMessageName",
            configPath: "MessageQueue:RabbitMQ:Config"
            );

        var inProcessConfig = (
            configurationSection: config.GetSection("MessageQueue:InProcess:Config"),
            simpleTargetName: "simpleTargetName2",
            simpleMessageName: "simpleMessageName2",
            configPath: "MessageQueue:InProcess:Config"
            );

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockHandler1 = mockRepo.Create<IMessageQueueHandler>();
        var mockHandler2 = mockRepo.Create<IMessageQueueHandler>();
        var mockPropertyResolver = mockRepo.Create<IMessagePropertyResolver>();
        var mockRabbitMQReceiver = mockRepo.Create<IMessageReceiverProvider>();
        var mockInProcessReceiver = mockRepo.Create<IMessageReceiverProvider>();
        var mockHandlerProvider = mockRepo.Create<IMessageHandlerProvider>();

        var services = new ServiceCollection();
        services.TryAddKeyedTransient(rabbitMQProvider.providerKey, (_, _) => mockRabbitMQReceiver.Object);
        services.TryAddKeyedTransient(inProcessProvider.providerKey, (_, _) => mockInProcessReceiver.Object);
        services.TryAddTransient(_ => mockHandlerProvider.Object);

        var serviceProvider = services.BuildServiceProvider();

        // Setup mock to return different providers for different handlers
        mockPropertyResolver.SetupSequence(s => s.ProviderSafe(It.IsAny<Type>(), It.IsAny<Type>()))
            .Returns(rabbitMQProvider)
            .Returns(inProcessProvider);

        mockPropertyResolver.SetupSequence(s => s.ConfigurationSafe(It.IsAny<Type>(), It.IsAny<Type>()))
            .Returns(rabbitMQConfig)
            .Returns(inProcessConfig);

        mockRabbitMQReceiver.Setup(s => s.SetHandlerProvider(mockHandlerProvider.Object))
            .Returns(mockRabbitMQReceiver.Object);
        mockInProcessReceiver.Setup(s => s.SetHandlerProvider(mockHandlerProvider.Object))
            .Returns(mockInProcessReceiver.Object);

        var factory = new MessageReceiverProviderFactory(
            [mockHandler1.Object, mockHandler2.Object],
            mockPropertyResolver.Object,
            serviceProvider,
            TestLogger.CreateLogger<MessageReceiverProviderFactory>()
            );

        var providers = factory.Create().ToArray();

        Assert.IsNotNull(providers);
        Assert.AreEqual(2, providers.Length, "Factory should return 2 providers (rabbitmq and in-process)");

        mockRepo.VerifyAll();
    }
}
