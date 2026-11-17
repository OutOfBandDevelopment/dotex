using System;
using OoBDev.TestUtilities.Containers;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;

namespace OoBDev.Amazon.Ses.Tests;

/// <summary>Sends through the Moto emulator; there is no test against real SES (see the project readme).</summary>
[TestClass]
[TestCategory(TestCategories.Integration)]
public class SesMotoIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed record Channel(string Name, string Protocol, string Provider) : IChannel;

    [IntegrationTestMethod("oobd-test-moto")]
    public async Task SendAsync_WithMoto_ReturnsASesMessageIdAsync()
    {
        // Stage
        var options = new SesOptions
        {
            Region = TestContext.GetRequiredProperty<string>("AWS_REGION"),
            AccessKeyId = TestContext.GetRequiredProperty<string>("AWS_ACCESS_KEY_ID"),
            SecretAccessKey = TestContext.GetRequiredProperty<string>("AWS_SECRET_ACCESS_KEY"),
            ServiceUrl = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("MOTO_URL")),
        };
        var factory = new SesClientFactory(Options.Create(options));
        var sender = $"test-{Guid.NewGuid():N}@example.test";
        using (var client = factory.Create())
        {
            await client.CreateEmailIdentityAsync(new CreateEmailIdentityRequest { EmailIdentity = sender }, TestContext.CancellationToken);
        }

        var provider = new SesChannelProvider(factory, Options.Create(options), TimeProvider.System, NullLogger<SesChannelProvider>.Instance);
        var message = new EmailMessage
        {
            From = sender,
            To = ["to@example.test"],
            Subject = "Moto",
            TextContent = "hello",
            Attachments = [EmailAttachment.FromBytes("a.txt", "text/plain", [65, 66])],
        };

        // Test
        var result = await provider.SendAsync(new Channel("mail", "email", AwsSesGlobals.ProviderName), message, TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(result.Success, result.Failure?.Message);
        Assert.IsFalse(string.IsNullOrEmpty(result.ProviderMessageId));
    }
}
