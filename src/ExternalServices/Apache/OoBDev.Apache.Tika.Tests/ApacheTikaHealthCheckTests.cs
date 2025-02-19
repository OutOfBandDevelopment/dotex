using OoBDev.TestUtilities;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Tests;

[TestClass]
public class ApacheTikaHealthCheckTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CheckHealthAsyncTest_Healthy()
    {
        var context = new HealthCheckContext();

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockClient = mockRepo.Create<IApacheTikaClient>();

        mockClient.Setup(s => s.GetHelloAsync()).Returns(Task.FromResult("From Test"));

        var check = new ApacheTikaHealthCheck(mockClient.Object);

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
        var mockClient = mockRepo.Create<IApacheTikaClient>();

        var check = new ApacheTikaHealthCheck(mockClient.Object);

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
