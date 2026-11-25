using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using OoBDev.MailKit.Services;
using OoBDev.TestUtilities;

namespace OoBDev.MailKit.Tests;

[TestClass]
public class MimeMessageFactoryReceivedTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToReceived_MapsAddressesBodyAndOrigin()
    {
        var mime = new MimeMessage
        {
            MessageId = "id-1@example.test",
            Subject = "Hello",
            Date = new DateTimeOffset(2026, 11, 5, 8, 0, 0, TimeSpan.Zero),
            Body = new TextPart("plain") { Text = "Hi" },
        };
        mime.From.Add(new MailboxAddress("Sender", "from@example.test"));
        mime.To.Add(new MailboxAddress("", "to@example.test"));
        mime.Cc.Add(new MailboxAddress("", "cc@example.test"));
        mime.ReplyTo.Add(new MailboxAddress("", "reply@example.test"));

        var email = new MimeMessageFactory(Options.Create(new MailKitSmtpClientOptions { Host = "smtp.example.test" })).ToReceived(mime, "imap.example.test:993", "INBOX");

        Assert.AreEqual("id-1@example.test", email.MessageId);
        Assert.AreEqual("Hello", email.Subject);
        Assert.AreEqual("Hi", email.TextContent);
        Assert.AreEqual(new DateTimeOffset(2026, 11, 5, 8, 0, 0, TimeSpan.Zero), email.Timestamp);
        StringAssert.Contains(email.From, "from@example.test");
        Assert.AreEqual(1, email.To.Length);
        Assert.AreEqual(1, email.Cc.Length);
        Assert.AreEqual(0, email.Bcc.Length);
        StringAssert.Contains(email.ReplyTo, "reply@example.test");
        Assert.AreEqual("imap.example.test:993", email.Metadata["server"]);
        Assert.AreEqual("INBOX", email.Metadata["path"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToReceived_AttachmentAndInlineImage_AreMappedWithContent()
    {
        var builder = new BodyBuilder { TextBody = "Hi", HtmlBody = "<img src=\"cid:logo@example.test\">" };
        builder.Attachments.Add("report.pdf", new byte[] { 1, 2, 3 }, ContentType.Parse("application/pdf"));
        var inline = (MimePart)builder.LinkedResources.Add("logo.png", new byte[] { 9, 8 }, ContentType.Parse("image/png"));
        inline.ContentId = "logo@example.test";
        var mime = new MimeMessage { Body = builder.ToMessageBody() };

        var email = new MimeMessageFactory(Options.Create(new MailKitSmtpClientOptions { Host = "smtp.example.test" })).ToReceived(mime, "s", "INBOX");

        Assert.HasCount(2, email.Attachments);
        var report = email.Attachments.Single(a => a.FileName == "report.pdf");
        var logo = email.Attachments.Single(a => a.FileName == "logo.png");
        Assert.AreEqual("report.pdf", report.FileName);
        Assert.AreEqual("application/pdf", report.ContentType);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, report.GetContentAsync().AsTask().Result.ToArray());
        Assert.AreEqual("logo@example.test", logo.ContentId);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToReceived_PlainTextOnly_HasNoAttachments()
    {
        var email = new MimeMessageFactory(Options.Create(new MailKitSmtpClientOptions { Host = "smtp.example.test" })).ToReceived(new MimeMessage { Body = new TextPart("plain") { Text = "x" } }, "s", "INBOX");

        Assert.IsEmpty(email.Attachments);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToReceived_NoSubjectOrId_UsesEmptySubjectAndGeneratedId()
    {
        var email = new MimeMessageFactory(Options.Create(new MailKitSmtpClientOptions { Host = "smtp.example.test" })).ToReceived(new MimeMessage(), "s", "INBOX");

        Assert.AreEqual(string.Empty, email.Subject);
        Assert.IsFalse(string.IsNullOrEmpty(email.MessageId));
        Assert.IsNull(email.From);
        Assert.IsNull(email.ReplyTo);
    }
}
