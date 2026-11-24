using System;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using OoBDev.Communications.Abstractions;
using OoBDev.MailKit.Services;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.MailKit.Tests;

/// <summary>The IMAP poll against the SMTP4Dev container: mail sent over SMTP is fetched once and, after it is acknowledged, not again.</summary>
[TestClass]
public class MailKitImapPollIntegrationTests
{
    private sealed record TestChannel(string Name, string Protocol, string Provider) : IChannel;

    public required TestContext TestContext { get; set; }

    [IntegrationTestMethod("oobd-test-smtp")]
    [TestCategory(TestCategories.Integration)]
    public async Task PollAsync_MailSentOverSmtp_IsFetchedOnceAfterAcknowledge()
    {
        // Stage
        var smtpHost = TestContext.GetRequiredProperty<string>("SMTP_HOST");
        var smtpPort = TestContext.GetPropertyOrDefault("SMTP_PORT", 25);
        var imapHost = TestContext.GetRequiredProperty<string>("IMAP_HOST");
        var imapPort = TestContext.GetPropertyOrDefault("IMAP_PORT", 143);
        var subject = $"Test_{Guid.NewGuid():N}";
        TestContext.WriteLine($"SMTP {smtpHost}:{smtpPort}, IMAP {imapHost}:{imapPort}, subject {subject}");
        var options = Options.Create(new MailKitImapClientOptions { Host = imapHost, Port = imapPort, SecureSocketOption = SecureSocketOptions.None, UserName = "test-user", Password = "test-password" });
        var provider = new MailKitImapChannelProvider(
            new ImapClientFactory(options),
            new MimeMessageFactory(Options.Create(new MailKitSmtpClientOptions { Host = smtpHost })),
            options,
            TimeProvider.System,
            NullLogger<MailKitImapChannelProvider>.Instance);
        var channel = new TestChannel("inbox", MailKitGlobals.EmailProtocol, MailKitGlobals.ImapProviderName);

        // Test
        using (var smtp = new SmtpClient())
        {
            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.None);
            await smtp.SendAsync(new MimeMessage
            {
                From = { InternetAddress.Parse("sender@example.test") },
                To = { InternetAddress.Parse("inbox@example.test") },
                Subject = subject,
                Body = new BodyBuilder { TextBody = "Poll me." }.ToMessageBody(),
            });
        }

        var first = (await provider.PollAsync(channel, 100)).Where(p => p.Message is IEmailMessage m && m.Subject == subject).ToList();
        TestContext.WriteLine($"First poll found {first.Count}");
        await provider.AcknowledgeAsync(channel, first.Select(p => p.Receipt).ToList());
        var second = (await provider.PollAsync(channel, 100)).Where(p => p.Message is IEmailMessage m && m.Subject == subject).ToList();
        TestContext.WriteLine($"Second poll found {second.Count}");

        // Assert
        Assert.HasCount(1, first);
        Assert.IsEmpty(second);
    }
}
