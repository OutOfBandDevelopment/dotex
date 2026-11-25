using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.ComplexEvents;

namespace OoBDev.Microsoft.Azure.EventHub.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class AzureEventHubTests
{
    public TestContext TestContext { get; set; } = null!;

    private const string FakeConnection = "Endpoint=sb://example.servicebus.windows.net/;SharedAccessKeyName=k;SharedAccessKey=c2VjcmV0;EntityPath=hub";

    [TestMethod]
    public async Task Register_ResolvesTheAzureClientAsTheHubClient()
    {
        var services = new ServiceCollection();
        services.TryAddAzureEventHub(o => o.ConnectionString = FakeConnection);
        await using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IEventHubClient>();
        TestContext.WriteLine($"resolved {client.GetType().Name}");

        Assert.IsInstanceOfType<AzureEventHubClient>(client);
        Assert.AreSame(client, provider.GetRequiredService<AzureEventHubClient>());
    }
}
