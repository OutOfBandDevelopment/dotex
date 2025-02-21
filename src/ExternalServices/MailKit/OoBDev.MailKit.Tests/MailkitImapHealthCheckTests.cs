using OoBDev.MailKit.Services;
using OoBDev.TestUtilities;
using MailKit.Net.Imap;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace OoBDev.MailKit.Tests;

[TestClass]
public class MailkitImapHealthCheckTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CheckHealthAsyncTest_Healthy()
    {
        var context = new HealthCheckContext();

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockClientFactory = mockRepo.Create<IImapClientFactory>();
        var mockClient = mockRepo.Create<IImapClient>();

        mockClientFactory.Setup(s => s.CreateAsync()).Returns(Task.FromResult(mockClient.Object));
        mockClient.Setup(s => s.Dispose());

        var check = new MailkitImapHealthCheck(mockClientFactory.Object);

        var result = await check.CheckHealthAsync(context);

        this.TestContext.WriteLine($"Status: {result.Status}");
        this.TestContext.WriteLine($"Description: {result.Description}");
        this.TestContext.WriteLine($"Exception: {result.Exception}");

        if (result.Data != null)
        {
            this.TestContext.WriteLine($"Data:");
            foreach (var item in result.Data)
            {
                this.TestContext.WriteLine($"\t{item.Key}: {item.Value}");
            }
        }

        Assert.AreEqual(HealthStatus.Healthy, result.Status);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CheckHealthAsyncTest_Degraded()
    {
        var context = new HealthCheckContext();

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockClientFactory = mockRepo.Create<IImapClientFactory>();

        var check = new MailkitImapHealthCheck(mockClientFactory.Object);

        var result = await check.CheckHealthAsync(context);

        this.TestContext.WriteLine($"Status: {result.Status}");
        this.TestContext.WriteLine($"Description: {result.Description}");
        this.TestContext.WriteLine($"Exception: {result.Exception}");

        if (result.Data != null)
        {
            this.TestContext.WriteLine($"Data:");
            foreach (var item in result.Data)
            {
                this.TestContext.WriteLine($"\t{item.Key}: {item.Value}");
            }
        }

        Assert.AreEqual(HealthStatus.Degraded, result.Status);

        mockRepo.VerifyAll();
    }
}
