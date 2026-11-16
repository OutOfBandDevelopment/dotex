using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;

namespace OoBDev.Amazon.Ses.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class SnsDeliveryWebhookProcessorTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed record Channel(string Name, string Protocol, string Provider) : IChannel;

    private static readonly IChannel Chan = new Channel("mail", "email", AwsSesGlobals.ProviderName);
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 0, 0, 0, TimeSpan.Zero);

    private static SnsDeliveryWebhookProcessor Processor(SesOptions? options = null) => new(Options.Create(options ?? new SesOptions()));

    private static WebhookRequest Envelope(string type, string inner)
    {
        var json = JsonSerializer.Serialize(new Dictionary<string, string> { ["Type"] = type, ["MessageId"] = "sns-1", ["TopicArn"] = "arn:aws:sns:us-east-1:1:events", ["Message"] = inner });
        return new WebhookRequest(Encoding.UTF8.GetBytes(json), new Dictionary<string, string>(), new Dictionary<string, string>(), Now);
    }

    [TestMethod]
    [DataRow("Send", DeliveryStatus.Sent)]
    [DataRow("Delivery", DeliveryStatus.Delivered)]
    [DataRow("Bounce", DeliveryStatus.Bounced)]
    [DataRow("Complaint", DeliveryStatus.Complained)]
    [DataRow("Reject", DeliveryStatus.Failed)]
    [DataRow("Open", DeliveryStatus.Opened)]
    [DataRow("Click", DeliveryStatus.Clicked)]
    public async Task ProcessAsync_SesEvent_MapsTheStatusAsync(string eventType, DeliveryStatus expected)
    {
        // Stage
        var inner = """{"eventType":"EVENT","mail":{"messageId":"ses-1","timestamp":"2026-11-05T10:00:00.000Z","tags":{"message-id":["m1"]}}}""".Replace("EVENT", eventType, StringComparison.Ordinal);

        // Test
        var events = await Processor().ProcessAsync(Chan, Envelope("Notification", inner), TestContext.CancellationToken);

        // Assert
        var evt = (DeliveryStatusEvent)events.Single();
        Assert.AreEqual("m1", evt.MessageId);
        Assert.AreEqual(expected, evt.Update.NewStatus);
        Assert.AreEqual(new DateTimeOffset(2026, 11, 5, 10, 0, 0, TimeSpan.Zero), evt.Update.Timestamp);
    }

    [TestMethod]
    public async Task ProcessAsync_NoTag_UsesTheSesMessageIdAsync()
    {
        // Test
        var events = await Processor().ProcessAsync(Chan, Envelope("Notification", """{"notificationType":"Bounce","bounce":{"bounceType":"Permanent"},"mail":{"messageId":"ses-9"}}"""), TestContext.CancellationToken);

        // Assert
        var evt = (DeliveryStatusEvent)events.Single();
        Assert.AreEqual("ses-9", evt.MessageId);
        Assert.AreEqual("Permanent", evt.Update.StatusDetails);
        Assert.AreEqual(Now, evt.Update.Timestamp);
    }

    [TestMethod]
    [DataRow("""{"notificationType":"Bounce","bounce":{"bounceType":"Permanent","bouncedRecipients":[{"emailAddress":"a@example.com"},{"emailAddress":"b@example.com"}]},"mail":{"messageId":"m"}}""", "a@example.com,b@example.com")]
    [DataRow("""{"notificationType":"Complaint","complaint":{"complainedRecipients":[{"emailAddress":"c@example.com"}]},"mail":{"messageId":"m"}}""", "c@example.com")]
    public async Task ProcessAsync_BounceAndComplaint_CarryTheRecipientsAsync(string inner, string expected)
    {
        var events = await Processor().ProcessAsync(Chan, Envelope("Notification", inner), TestContext.CancellationToken);

        var update = ((DeliveryStatusEvent)events.Single()).Update;
        Assert.AreEqual(expected, string.Join(",", update.Recipients!));
    }

    [TestMethod]
    public async Task ProcessAsync_SubscriptionConfirmationOrUnknownEvent_YieldsNothingAsync()
    {
        // Test
        var confirmation = await Processor().ProcessAsync(Chan, Envelope("SubscriptionConfirmation", "token"), TestContext.CancellationToken);
        var delay = await Processor().ProcessAsync(Chan, Envelope("Notification", """{"eventType":"DeliveryDelay","mail":{"messageId":"x"}}"""), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(0, confirmation.Count);
        Assert.AreEqual(0, delay.Count);
    }

    [TestMethod]
    public void ValidateSignature_UnsignedOrMalformed_IsFalse()
    {
        // Test
        var unsigned = Processor().ValidateSignature(Chan, Envelope("Notification", "{}"));
        var garbage = Processor().ValidateSignature(Chan, new WebhookRequest(Encoding.UTF8.GetBytes("not json"), new Dictionary<string, string>(), new Dictionary<string, string>(), Now));
        var wrongTopic = Processor(new SesOptions { AllowedTopicArns = ["arn:aws:sns:us-east-1:1:other"] }).ValidateSignature(Chan, Envelope("Notification", "{}"));

        // Assert
        Assert.IsFalse(unsigned);
        Assert.IsFalse(garbage);
        Assert.IsFalse(wrongTopic);
    }

    [TestMethod]
    public void TryAddAmazonSesServices_WithSection_RegistersTheProviderAndProcessor()
    {
        // Stage
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["SesOptions:Region"] = "eu-west-1" }).Build();

        // Test
        var provider = new ServiceCollection().AddLogging().TryAddAmazonSesServices(config).BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<SesChannelProvider>(provider.GetServices<IChannelProvider>().Single());
        Assert.IsInstanceOfType<SnsDeliveryWebhookProcessor>(provider.GetServices<IWebhookProcessor>().Single());
        Assert.AreEqual("eu-west-1", provider.GetRequiredService<IOptions<SesOptions>>().Value.Region);
    }

    [TestMethod]
    public void TryAddAmazonSesServices_WithoutSection_RegistersNothing()
    {
        // Test
        var provider = new ServiceCollection().TryAddAmazonSesServices(new ConfigurationBuilder().Build()).BuildServiceProvider();

        // Assert
        Assert.AreEqual(0, provider.GetServices<IChannelProvider>().Count());
    }
}
