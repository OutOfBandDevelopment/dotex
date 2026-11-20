using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DataLoader.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class RegistrationTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task TryAddDataLoader_ResolvesLoaderThatReadsCsvAndJson()
    {
        var sink = new InMemoryRowSink("id");
        var services = new ServiceCollection();
        services.TryAddDataLoader();
        services.TryAddDataLoader();
        services.AddSingleton<IRowSink>(sink);
        using var provider = services.BuildServiceProvider();

        var loader = provider.GetRequiredService<ILoader>();
        using var csv = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Ann\n"));
        using var json = new MemoryStream(Encoding.UTF8.GetBytes("[{\"id\":2,\"name\":\"Bo\"}]"));
        await loader.LoadAsync(csv, "csv", new LoadOptions(), CancellationToken.None);
        await loader.LoadAsync(json, "json", new LoadOptions(), CancellationToken.None);
        TestContext.WriteLine($"rows: {sink.Rows.Count}");

        Assert.AreEqual(2, sink.Rows.Count);
        Assert.AreEqual(2, provider.GetServices<IRowReader>().Count(), "readers registered once each");
    }
}
