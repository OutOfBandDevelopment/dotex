using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;

namespace OoBDev.SendGrid.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class SendGridChannelProviderTests
{
    private sealed record Channel(string Name, string Protocol, string Provider) : IChannel;

    private sealed class Attachment : IEmailAttachment
    {
        public string FileName => "a.txt";

        public string ContentType => "text/plain";

        public string? ContentId => null;

        public ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult<ReadOnlyMemory<byte>>(new byte[] { 1, 2, 3 });
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return respond(request);
        }
    }

    private static readonly Channel Chan = new("support-email", "email", "sendgrid");

    private static SendGridChannelProvider Create(Handler handler, string? defaultFrom = "noreply@example.test")
        => new(new HttpClient(handler),
            Options.Create(new SendGridOptions { ApiKey = "key-1", DefaultFrom = defaultFrom }),
            new FakeTimeProvider(),
            NullLogger<SendGridChannelProvider>.Instance);

    private static EmailMessage Mail() => new() { To = ["a@example.test"], Subject = "Hi", TextContent = "Hello" };

    private static HttpResponseMessage Accepted()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Accepted);
        response.Headers.Add("X-Message-Id", "sg-42");
        return response;
    }

    [TestMethod]
    public async Task SendAsync_Accepted_ReturnsVendorIdAndBuildsRequest()
    {
        // Stage
        var handler = new Handler(_ => Accepted());
        var provider = Create(handler);

        // Test
        var result = await provider.SendAsync(Chan, Mail());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("sg-42", result.ProviderMessageId);
        Assert.AreEqual("Bearer", handler.Request!.Headers.Authorization!.Scheme);
        Assert.AreEqual("key-1", handler.Request.Headers.Authorization.Parameter);
        Assert.AreEqual("https://api.sendgrid.com/v3/mail/send", handler.Request.RequestUri!.ToString());
        var json = JsonNode.Parse(handler.Body!)!;
        Assert.AreEqual("noreply@example.test", (string?)json["from"]!["email"]);
        Assert.AreEqual("a@example.test", (string?)json["personalizations"]![0]!["to"]![0]!["email"]);
        Assert.AreEqual("Hi", (string?)json["subject"]);
        Assert.AreEqual("text/plain", (string?)json["content"]![0]!["type"]);
    }

    [TestMethod]
    public async Task SendAsync_AllFields_MapsCcBccReplyToHtmlAndAttachment()
    {
        // Stage
        var handler = new Handler(_ => Accepted());
        var message = new EmailMessage
        {
            From = "me@example.test",
            To = ["a@example.test"],
            Cc = ["c@example.test"],
            Bcc = ["b@example.test"],
            ReplyTo = "r@example.test",
            Subject = "S",
            HtmlContent = "<b>x</b>",
            Attachments = [new Attachment()],
        };

        // Test
        var result = await Create(handler, defaultFrom: null).SendAsync(Chan, message);

        // Assert
        Assert.IsTrue(result.Success);
        var json = JsonNode.Parse(handler.Body!)!;
        Assert.AreEqual("me@example.test", (string?)json["from"]!["email"]);
        Assert.AreEqual("c@example.test", (string?)json["personalizations"]![0]!["cc"]![0]!["email"]);
        Assert.AreEqual("b@example.test", (string?)json["personalizations"]![0]!["bcc"]![0]!["email"]);
        Assert.AreEqual("r@example.test", (string?)json["reply_to"]!["email"]);
        Assert.AreEqual("text/html", (string?)json["content"]![0]!["type"]);
        Assert.AreEqual("AQID", (string?)json["attachments"]![0]!["content"]);
        Assert.AreEqual("attachment", (string?)json["attachments"]![0]!["disposition"]);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.TooManyRequests, SendFailureKind.RateLimited)]
    [DataRow(HttpStatusCode.InternalServerError, SendFailureKind.Transient)]
    [DataRow(HttpStatusCode.RequestTimeout, SendFailureKind.Transient)]
    [DataRow(HttpStatusCode.BadRequest, SendFailureKind.Permanent)]
    [DataRow(HttpStatusCode.Unauthorized, SendFailureKind.Permanent)]
    public async Task SendAsync_VendorError_ClassifiesWithoutSecretsOrContent(HttpStatusCode status, SendFailureKind expected)
    {
        // Stage
        var handler = new Handler(_ => new HttpResponseMessage(status) { Content = new StringContent("key-1 a@example.test") });

        // Test
        var result = await Create(handler).SendAsync(Chan, Mail());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(expected, result.Failure!.Kind);
        Assert.AreEqual((int)status, result.Failure.StatusCode);
        Assert.IsFalse(result.Failure.Message.Contains("key-1", StringComparison.Ordinal));
        Assert.IsFalse(result.Failure.Message.Contains("a@example.test", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task SendAsync_RateLimitedWithRetryAfter_CarriesTheDelay()
    {
        // Stage
        var handler = new Handler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            response.Headers.RetryAfter = new global::System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
            return response;
        });

        // Test
        var result = await Create(handler).SendAsync(Chan, Mail());

        // Assert
        Assert.AreEqual(TimeSpan.FromSeconds(30), result.Failure!.RetryAfter);
    }

    [TestMethod]
    public async Task SendAsync_NetworkError_IsTransient()
    {
        // Stage
        var handler = new Handler(_ => throw new HttpRequestException("down"));

        // Test
        var result = await Create(handler).SendAsync(Chan, Mail());

        // Assert
        Assert.AreEqual(SendFailureKind.Transient, result.Failure!.Kind);
    }

    [TestMethod]
    public async Task SendAsync_NoSenderAnywhere_IsRejectedWithoutCallingTheVendor()
    {
        // Stage
        var handler = new Handler(_ => Accepted());

        // Test
        var result = await Create(handler, defaultFrom: null).SendAsync(Chan, Mail());

        // Assert
        Assert.AreEqual(SendFailureKind.Rejected, result.Failure!.Kind);
        Assert.IsNull(handler.Request);
    }

    [TestMethod]
    public async Task SendAsync_NoRecipient_IsRejected()
    {
        // Stage
        var handler = new Handler(_ => Accepted());

        // Test
        var result = await Create(handler).SendAsync(Chan, new EmailMessage { Subject = "S" });

        // Assert
        Assert.AreEqual(SendFailureKind.Rejected, result.Failure!.Kind);
        Assert.IsNull(handler.Request);
    }

    [TestMethod]
    public async Task SendAsync_NotAnEmail_IsRejectedAndCannotBeSent()
    {
        // Stage
        var handler = new Handler(_ => Accepted());
        var provider = Create(handler);
        var sms = new SmsMessage { To = "+15551234567", Body = "x" };

        // Test
        var can = await provider.CanSendAsync(Chan, sms);
        var result = await provider.SendAsync(Chan, sms);

        // Assert
        Assert.IsFalse(can);
        Assert.AreEqual(SendFailureKind.Rejected, result.Failure!.Kind);
        Assert.AreEqual("sendgrid", provider.ProviderName);
        CollectionAssert.AreEqual(new[] { "email" }, provider.SupportedProtocols);
    }

    [TestMethod]
    public void TryAddSendGridProvider_WithKey_RegistersTheProvider()
    {
        // Stage
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OoBDev:SendGrid:ApiKey"] = "k",
        }).Build();
        var services = new ServiceCollection().AddLogging();

        // Test
        services.TryAddSendGridProvider(configuration, SendGridGlobals.ConfigurationSection);
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsTrue(provider.GetServices<IChannelProvider>().Any(p => p.ProviderName == "sendgrid"));
    }

    [TestMethod]
    public void TryAddSendGridProvider_WithoutKey_RegistersNothing()
    {
        // Stage
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        // Test
        services.TryAddSendGridProvider(configuration, SendGridGlobals.ConfigurationSection);

        // Assert
        Assert.AreEqual(0, services.Count);
    }
}
