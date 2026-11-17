using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating.EntityFrameworkCore.Tests;

[TestClass]
public class EfTemplateTests
{
    private SqliteTemplatesDatabase database = null!;
    private EfTemplateRepository repository = null!;

    [TestInitialize]
    public void Initialize()
    {
        database = new SqliteTemplatesDatabase();
        repository = new EfTemplateRepository(database.Factory, TimeProvider.System);
    }

    [TestCleanup]
    public async Task Cleanup() => await database.DisposeAsync();

    private static TemplatePublish Publish(string content, string culture = "", string tenant = "") =>
        new("welcome", "text/x-scriban", content, "tester", culture, tenant);

    private EfTemplateSource Source(string tenant = "") =>
        new(repository, Options.Create(new EfTemplateSourceOptions { TenantId = tenant }));

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Publish_Twice_VersionsRiseAndLatestWins()
    {
        // Test
        var first = await repository.PublishAsync(Publish("one"));
        var second = await repository.PublishAsync(Publish("two"));
        var latest = await repository.GetAsync("welcome");
        var specific = await repository.GetAsync("welcome", version: 1);

        // Assert
        Assert.AreEqual(1, first.Version);
        Assert.AreEqual(2, second.Version);
        Assert.AreEqual("two", latest!.Content);
        Assert.AreEqual("one", specific!.Content);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Get_TenantOverride_PrefersTenantThenFallsBackToShared()
    {
        // Stage
        await repository.PublishAsync(Publish("shared"));
        await repository.PublishAsync(Publish("acme", tenant: "acme"));

        // Test / Assert
        Assert.AreEqual("acme", (await repository.GetAsync("welcome", tenantId: "acme"))!.Content);
        Assert.AreEqual("shared", (await repository.GetAsync("welcome", tenantId: "other"))!.Content);
        Assert.AreEqual("shared", (await repository.GetAsync("welcome"))!.Content);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Rollback_PublishesOldContentAsNewVersion()
    {
        // Stage
        await repository.PublishAsync(Publish("one"));
        await repository.PublishAsync(Publish("two"));

        // Test
        var restored = await repository.RollbackAsync("welcome", 1, "ops");
        var history = await repository.GetHistoryAsync("welcome");

        // Assert
        Assert.AreEqual(3, restored.Version);
        Assert.AreEqual("one", restored.Content);
        Assert.AreEqual(3, history.Count);
        Assert.AreEqual(3, history[0].Version);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Rollback_UnknownVersion_Throws()
    {
        await repository.PublishAsync(Publish("one"));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => repository.RollbackAsync("welcome", 9, "ops"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Withdraw_HidesTemplateButKeepsHistory()
    {
        // Stage
        await repository.PublishAsync(Publish("one"));

        // Test
        var count = await repository.WithdrawAsync("welcome");

        // Assert
        Assert.AreEqual(1, count);
        Assert.IsNull(await repository.GetAsync("welcome"));
        Assert.AreEqual(1, (await repository.GetHistoryAsync("welcome")).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Publish_MissingFields_Throw()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => repository.PublishAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => repository.PublishAsync(Publish("x") with { CreatedBy = " " }));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Source_Get_ReturnsContextWithContentAndTarget()
    {
        // Stage
        await repository.PublishAsync(Publish("hello {{ name }}") with { TargetContentType = "text/html", TargetFileExtension = ".html" });

        // Test
        var context = Source().Get("welcome").Single();
        using var reader = new StreamReader(context.OpenTemplate(context));

        // Assert
        Assert.AreEqual("text/x-scriban", context.TemplateContentType);
        Assert.AreEqual("text/html", context.TargetContentType);
        Assert.AreEqual(".html", context.TargetFileExtension);
        Assert.AreEqual("hello {{ name }}", await reader.ReadToEndAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Source_Get_CultureSuffixAndVersionSuffix()
    {
        // Stage
        await repository.PublishAsync(Publish("english"));
        await repository.PublishAsync(Publish("english v2"));
        await repository.PublishAsync(Publish("espanol", culture: "es-ES"));

        // Test
        string Read(string name)
        {
            var c = Source().Get(name).Single();
            using var r = new StreamReader(c.OpenTemplate(c));
            return r.ReadToEnd();
        }

        // Assert
        Assert.AreEqual("espanol", Read("welcome.es-ES"));
        Assert.AreEqual("english v2", Read("welcome"));
        Assert.AreEqual("english", Read("welcome@1"));
        Assert.AreEqual(0, Source().Get("missing").Count());
        Assert.AreEqual(0, Source().Get("welcome.zz-notaculture").Count());
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Source_Get_UsesConfiguredTenant()
    {
        // Stage
        await repository.PublishAsync(Publish("shared"));
        await repository.PublishAsync(Publish("acme", tenant: "acme"));

        // Test
        var c = Source("acme").Get("welcome").Single();
        using var r = new StreamReader(c.OpenTemplate(c));

        // Assert
        Assert.AreEqual("acme", await r.ReadToEndAsync());
    }
}
