using OoBDev.TestUtilities;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OllamaSharp;
using OllamaSharp.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Ollama.Tests;

[TestClass]
public class OllamaHealthCheckTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CheckHealthAsyncTest_Healthy()
    {
        var context = new HealthCheckContext();

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockClient = mockRepo.Create<IOllamaApiClient>();

        mockClient.Setup(s => s.ListLocalModels(It.IsAny<CancellationToken>())).Returns(Task.FromResult(Enumerable.Empty<Model>()));

        var check = new OllamaHealthCheck(mockClient.Object);

        var result = await check.CheckHealthAsync(context);

        TestContext.WriteLine($"Status: {result.Status}");
        TestContext.WriteLine($"Description: {result.Description}");
        TestContext.WriteLine($"Exception: {result.Exception}");

        if (result.Data != null)
        {
            TestContext.WriteLine($"Data:");
            foreach (var item in result.Data)
            {
                TestContext.WriteLine($"\t{item.Key}: {item.Value}");
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
        var mockClient = mockRepo.Create<IOllamaApiClient>();

        var check = new OllamaHealthCheck(mockClient.Object);

        var result = await check.CheckHealthAsync(context);

        TestContext.WriteLine($"Status: {result.Status}");
        TestContext.WriteLine($"Description: {result.Description}");
        TestContext.WriteLine($"Exception: {result.Exception}");

        if (result.Data != null)
        {
            TestContext.WriteLine($"Data:");
            foreach (var item in result.Data)
            {
                TestContext.WriteLine($"\t{item.Key}: {item.Value}");
            }
        }

        Assert.AreEqual(HealthStatus.Degraded, result.Status);

        mockRepo.VerifyAll();
    }
}
