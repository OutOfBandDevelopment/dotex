using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.MessageQueueing.Services;
using System;
using System.Collections.Generic;

namespace OoBDev.MessageQueueing.Tests.Services;

[TestClass]
public class MessagePropertyResolverTests
{
    private sealed class OrdersChannel;

    private sealed class OrderPlaced;

    [MessageQueue("renamed-channel")]
    private sealed class NamedChannel;

    [MessageQueue("renamed-message")]
    private sealed class NamedMessage;

    private static MessagePropertyResolver Resolver(params (string Key, string Value)[] values)
    {
        var data = new Dictionary<string, string?>();
        foreach (var (key, value) in values)
        {
            data[key] = value;
        }

        return new MessagePropertyResolver(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ProviderSafe_AllFourSectionsPresent_ChannelAndMessageWins()
    {
        // Stage
        var resolver = Resolver(
            ("MessageQueue:OrdersChannel:OrderPlaced:Provider", "exact"),
            ("MessageQueue:OrderPlaced:Provider", "message"),
            ("MessageQueue:OrdersChannel:Provider", "channel"),
            ("MessageQueue:Default:Provider", "default"));

        // Test
        var (providerKey, target, message, path) = resolver.ProviderSafe(typeof(OrdersChannel), typeof(OrderPlaced));

        // Assert
        Assert.AreEqual("exact", providerKey);
        Assert.AreEqual("OrdersChannel", target);
        Assert.AreEqual("OrderPlaced", message);
        Assert.AreEqual("MessageQueue:OrdersChannel:OrderPlaced", path);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ProviderSafe_ExactMissing_FallsBackToMessageThenChannelThenDefault()
    {
        // Test
        var message = Resolver(("MessageQueue:OrderPlaced:Provider", "message"), ("MessageQueue:OrdersChannel:Provider", "channel"), ("MessageQueue:Default:Provider", "default"))
            .ProviderSafe(typeof(OrdersChannel), typeof(OrderPlaced)).providerKey;
        var channel = Resolver(("MessageQueue:OrdersChannel:Provider", "channel"), ("MessageQueue:Default:Provider", "default"))
            .ProviderSafe(typeof(OrdersChannel), typeof(OrderPlaced)).providerKey;
        var fallback = Resolver(("MessageQueue:Default:Provider", "default"))
            .ProviderSafe(typeof(OrdersChannel), typeof(OrderPlaced)).providerKey;

        // Assert
        Assert.AreEqual("message", message);
        Assert.AreEqual("channel", channel);
        Assert.AreEqual("default", fallback);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ProviderSafe_NothingConfigured_ReturnsNullKeyAndPath()
    {
        // Test
        var (providerKey, _, _, path) = Resolver().ProviderSafe(typeof(OrdersChannel), typeof(OrderPlaced));

        // Assert
        Assert.IsNull(providerKey);
        Assert.IsNull(path);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ProviderSafe_AttributeNames_OverrideTheTypeNames()
    {
        // Stage
        var resolver = Resolver(("MessageQueue:renamed-channel:renamed-message:Provider", "attr"));

        // Test
        var (providerKey, target, message, _) = resolver.ProviderSafe(typeof(NamedChannel), typeof(NamedMessage));

        // Assert
        Assert.AreEqual("attr", providerKey);
        Assert.AreEqual("renamed-channel", target);
        Assert.AreEqual("renamed-message", message);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ProviderSafe_ObjectChannel_UsesDefaultAsTheTargetName()
    {
        // Test
        var (_, target, _, _) = Resolver().ProviderSafe(typeof(object), typeof(OrderPlaced));

        // Assert
        Assert.AreEqual("Default", target);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ConfigurationSafe_ConfigChild_ReturnsTheConfigSection()
    {
        // Stage
        var resolver = Resolver(
            ("MessageQueue:OrdersChannel:OrderPlaced:Provider", "p"),
            ("MessageQueue:OrdersChannel:OrderPlaced:Config:QueueName", "orders"));

        // Test
        var (section, _, _, path) = resolver.ConfigurationSafe(typeof(OrdersChannel), typeof(OrderPlaced));

        // Assert
        Assert.AreEqual("orders", section!["QueueName"]);
        Assert.AreEqual("MessageQueue:OrdersChannel:OrderPlaced:Config", path);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ConfigurationSafe_NoConfigChild_ReturnsTheSelectedSection()
    {
        // Test
        var (section, _, _, _) = Resolver(("MessageQueue:OrdersChannel:OrderPlaced:QueueName", "flat"))
            .ConfigurationSafe(typeof(OrdersChannel), typeof(OrderPlaced));

        // Assert
        Assert.AreEqual("flat", section!["QueueName"]);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Configuration_NothingConfigured_ThrowsWithTheExpectedPath()
    {
        // Test
        var ex = Assert.Throws<InvalidOperationException>(() => Resolver().Configuration(typeof(OrdersChannel), typeof(OrderPlaced)));

        // Assert
        StringAssert.Contains(ex.Message, "MessageQueue:OrdersChannel:OrderPlaced");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Provider_NothingConfigured_Throws()
    {
        // Test
        var ex = Assert.Throws<InvalidOperationException>(() => Resolver().Provider(typeof(OrdersChannel), typeof(OrderPlaced)));

        // Assert
        StringAssert.Contains(ex.Message, ":Provider");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void MessageId_BlankOrGiven_GeneratesOrKeeps()
    {
        // Stage
        var resolver = Resolver();

        // Test
        var kept = resolver.MessageId(typeof(OrdersChannel), typeof(OrderPlaced), "abc");
        var generated = resolver.MessageId(typeof(OrdersChannel), typeof(OrderPlaced), " ");

        // Assert
        Assert.AreEqual("abc", kept);
        Assert.IsTrue(Guid.TryParse(generated, out _));
    }
}
