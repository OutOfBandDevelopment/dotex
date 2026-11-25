using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests;

[TestClass]
public class SuppressingSendServiceTests
{
    private static SendResult Ok(IMessage m) => new(true, m.MessageId, m.CorrelationId, "p", DateTimeOffset.UnixEpoch);

    private static (Mock<ISendService> Inner, List<IEmailMessage> Sent) Inner()
    {
        var sent = new List<IEmailMessage>();
        var inner = new Mock<ISendService>();
        inner.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<IMessage>(), It.IsAny<SendOptions?>(), It.IsAny<CancellationToken>()))
            .Callback<string, IMessage, SendOptions?, CancellationToken>((_, m, _, _) => sent.Add((IEmailMessage)m))
            .ReturnsAsync((string _, IMessage m, SendOptions? _, CancellationToken _) => Ok(m));
        return (inner, sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_DropsSuppressedRecipientsAndKeepsTheRest()
    {
        var (inner, sent) = Inner();
        var list = new InMemorySuppressionList();
        await list.AddAsync("Bad@Example.com", "bounce");
        var service = new SuppressingSendService(inner.Object, list, new FakeTimeProvider());

        var result = await service.SendAsync("mail", new EmailMessage { To = ["bad@example.com", "ok@example.com"], Cc = ["bad@example.com"], Subject = "s" });

        Assert.IsTrue(result.Success);
        CollectionAssert.AreEqual(new[] { "ok@example.com" }, sent[0].To);
        Assert.IsEmpty(sent[0].Cc);
        Assert.AreEqual("s", sent[0].Subject);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_AllRecipientsSuppressed_RejectsWithoutSending()
    {
        var (inner, sent) = Inner();
        var list = new InMemorySuppressionList();
        await list.AddAsync("bad@example.com", "complaint");

        var result = await new SuppressingSendService(inner.Object, list, new FakeTimeProvider())
            .SendAsync("mail", new EmailMessage { To = ["bad@example.com"] });

        Assert.IsFalse(result.Success);
        Assert.AreEqual(SendFailureKind.Rejected, result.Failure!.Kind);
        Assert.IsEmpty(sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_RemovedAddress_IsSentAgain()
    {
        var (inner, sent) = Inner();
        var list = new InMemorySuppressionList();
        await list.AddAsync("a@example.com", "bounce");
        await list.RemoveAsync("A@example.com");

        await new SuppressingSendService(inner.Object, list, new FakeTimeProvider()).SendAsync("mail", new EmailMessage { To = ["a@example.com"] });

        Assert.HasCount(1, sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_NonEmailMessage_PassesThrough()
    {
        var inner = new Mock<ISendService>();
        var sms = new Mock<IMessage>();
        sms.SetupGet(m => m.MessageId).Returns("m");
        inner.Setup(s => s.SendAsync("sms", sms.Object, null, default)).ReturnsAsync(Ok(sms.Object));

        var result = await new SuppressingSendService(inner.Object, new InMemorySuppressionList(), new FakeTimeProvider()).SendAsync("sms", sms.Object);

        Assert.IsTrue(result.Success);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddEmailSuppression_WithoutSendService_Throws() =>
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddEmailSuppression());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddEmailSuppression_WrapsTheRegisteredSendService()
    {
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new Mock<ISendService>().Object);
        services.AddEmailSuppression();

        using var provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<SuppressingSendService>(provider.GetRequiredService<ISendService>());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(DeliveryStatus.Bounced, "Permanent", true)]
    [DataRow(DeliveryStatus.Complained, "abuse", true)]
    [DataRow(DeliveryStatus.Bounced, "Transient", false)]
    [DataRow(DeliveryStatus.Delivered, null, false)]
    public async Task StatusHandler_SuppressesOnlyHardBouncesAndComplaints(DeliveryStatus status, string? details, bool suppressed)
    {
        var list = new InMemorySuppressionList();

        await new SuppressionStatusHandler(list).HandleAsync(new DeliveryStatusUpdate("m", status, details, DateTimeOffset.UnixEpoch, ["a@example.com"]));

        Assert.AreEqual(suppressed, await list.IsSuppressedAsync("a@example.com"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task StatusHandler_NoRecipients_DoesNothing()
    {
        var list = new InMemorySuppressionList();

        await new SuppressionStatusHandler(list).HandleAsync(new DeliveryStatusUpdate("m", DeliveryStatus.Complained, null, DateTimeOffset.UnixEpoch));

        Assert.IsFalse(await list.IsSuppressedAsync("a@example.com"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddEmailSuppression_RegistersTheStatusHandler()
    {
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new Mock<ISendService>().Object);
        services.AddEmailSuppression();

        using var provider = services.BuildServiceProvider();

        Assert.IsTrue(provider.GetServices<IDeliveryStatusHandler>().OfType<SuppressionStatusHandler>().Any());
    }
}
