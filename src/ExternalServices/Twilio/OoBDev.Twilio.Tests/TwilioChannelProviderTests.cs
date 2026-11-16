using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

namespace OoBDev.Twilio.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class TwilioChannelProviderTests
{
    private sealed record Channel(string Name, string Protocol, string Provider) : IChannel;

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

    private static readonly Channel Chan = new("alerts-sms", "sms", "twilio");

    private static TwilioChannelProvider Create(Handler handler, string? defaultFrom = "+15550000000")
        => new(new HttpClient(handler),
            Options.Create(new TwilioOptions { AccountSid = "AC123", AuthToken = "tok-1", DefaultFrom = defaultFrom }),
            new FakeTimeProvider(),
            NullLogger<TwilioChannelProvider>.Instance);

    private static SmsMessage Text() => new() { To = "+15551234567", Body = "Hello there" };

    private static HttpResponseMessage Created(string json = """{"sid":"SM42"}""")
        => new(HttpStatusCode.Created) { Content = new StringContent(json) };

    [TestMethod]
    public async Task SendAsync_Created_ReturnsSidAndBuildsRequest()
    {
        // Stage
        var handler = new Handler(_ => Created());

        // Test
        var result = await Create(handler).SendAsync(Chan, Text());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("SM42", result.ProviderMessageId);
        Assert.AreEqual("https://api.twilio.com/2010-04-01/Accounts/AC123/Messages.json", handler.Request!.RequestUri!.ToString());
        Assert.AreEqual("Basic", handler.Request.Headers.Authorization!.Scheme);
        Assert.AreEqual("AC123:tok-1", Encoding.UTF8.GetString(Convert.FromBase64String(handler.Request.Headers.Authorization.Parameter!)));
        StringAssert.Contains(handler.Body!, "To=%2B15551234567", StringComparison.Ordinal);
        StringAssert.Contains(handler.Body!, "From=%2B15550000000", StringComparison.Ordinal);
        StringAssert.Contains(handler.Body!, "Body=Hello+there", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task SendAsync_CreatedWithUnreadableBody_StillSucceedsWithoutId()
    {
        // Stage
        var handler = new Handler(_ => Created("not json"));

        // Test
        var result = await Create(handler).SendAsync(Chan, Text());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.ProviderMessageId);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.TooManyRequests, SendFailureKind.RateLimited)]
    [DataRow(HttpStatusCode.ServiceUnavailable, SendFailureKind.Transient)]
    [DataRow(HttpStatusCode.RequestTimeout, SendFailureKind.Transient)]
    [DataRow(HttpStatusCode.BadRequest, SendFailureKind.Permanent)]
    [DataRow(HttpStatusCode.Unauthorized, SendFailureKind.Permanent)]
    public async Task SendAsync_VendorError_ClassifiesWithoutSecretsOrNumbers(HttpStatusCode status, SendFailureKind expected)
    {
        // Stage
        var handler = new Handler(_ => new HttpResponseMessage(status) { Content = new StringContent("tok-1 +15551234567") });

        // Test
        var result = await Create(handler).SendAsync(Chan, Text());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(expected, result.Failure!.Kind);
        Assert.AreEqual((int)status, result.Failure.StatusCode);
        Assert.IsFalse(result.Failure.Message.Contains("tok-1", StringComparison.Ordinal));
        Assert.IsFalse(result.Failure.Message.Contains("5551234567", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task SendAsync_NetworkError_IsTransient()
    {
        // Stage
        var handler = new Handler(_ => throw new HttpRequestException("down"));

        // Test
        var result = await Create(handler).SendAsync(Chan, Text());

        // Assert
        Assert.AreEqual(SendFailureKind.Transient, result.Failure!.Kind);
    }

    [TestMethod]
    public async Task SendAsync_NoSenderAnywhere_IsRejectedWithoutCallingTheVendor()
    {
        // Stage
        var handler = new Handler(_ => Created());

        // Test
        var result = await Create(handler, defaultFrom: null).SendAsync(Chan, Text());

        // Assert
        Assert.AreEqual(SendFailureKind.Rejected, result.Failure!.Kind);
        Assert.IsNull(handler.Request);
    }

    [TestMethod]
    public async Task SendAsync_NoRecipient_IsRejected()
    {
        // Stage
        var handler = new Handler(_ => Created());

        // Test
        var result = await Create(handler).SendAsync(Chan, new SmsMessage { Body = "x" });

        // Assert
        Assert.AreEqual(SendFailureKind.Rejected, result.Failure!.Kind);
        Assert.IsNull(handler.Request);
    }

    [TestMethod]
    public async Task SendAsync_NotAText_IsRejectedAndCannotBeSent()
    {
        // Stage
        var provider = Create(new Handler(_ => Created()));
        var email = new EmailMessage { To = ["a@example.test"] };

        // Test
        var can = await provider.CanSendAsync(Chan, email);
        var result = await provider.SendAsync(Chan, email);

        // Assert
        Assert.IsFalse(can);
        Assert.AreEqual(SendFailureKind.Rejected, result.Failure!.Kind);
        Assert.AreEqual("twilio", provider.ProviderName);
        CollectionAssert.AreEqual(new[] { "sms" }, provider.SupportedProtocols);
    }

    [TestMethod]
    public void TryAddTwilioProvider_WithSid_RegistersTheProvider()
    {
        // Stage
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OoBDev:Twilio:AccountSid"] = "AC1",
            ["OoBDev:Twilio:AuthToken"] = "t",
        }).Build();
        var services = new ServiceCollection().AddLogging();

        // Test
        services.TryAddTwilioProvider(configuration, TwilioGlobals.ConfigurationSection);
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsTrue(provider.GetServices<IChannelProvider>().Any(p => p.ProviderName == "twilio"));
    }

    [TestMethod]
    public void TryAddTwilioProvider_WithoutSid_RegistersNothing()
    {
        // Stage
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        // Test
        services.TryAddTwilioProvider(configuration, TwilioGlobals.ConfigurationSection);

        // Assert
        Assert.AreEqual(0, services.Count);
    }
}
