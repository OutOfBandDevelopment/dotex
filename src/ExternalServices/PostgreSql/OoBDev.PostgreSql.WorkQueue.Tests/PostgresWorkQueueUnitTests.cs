using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WorkQueue;

namespace OoBDev.PostgreSql.WorkQueue.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class PostgresWorkQueueUnitTests
{
    [TestMethod]
    public void CreateScript_UsesTheConfiguredNames()
    {
        var script = PostgresWorkQueueSchema.CreateScript(new PostgresWorkQueueOptions { Schema = "jobs", TasksTable = "items", DeadLettersTable = "graveyard" });

        StringAssert.Contains(script, "CREATE SCHEMA IF NOT EXISTS \"jobs\"");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS \"jobs\".\"items\"");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS \"jobs\".\"graveyard\"");
        StringAssert.Contains(script, "PRIMARY KEY (stage, item_key)");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS \"jobs\".\"worker_events\"");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS \"jobs\".\"worker_events_reader\"");
    }

    [TestMethod]
    [DataRow("work_queue", true)]
    [DataRow("_x1", true)]
    [DataRow("Tasks", false)]
    [DataRow("a;drop table x", false)]
    [DataRow("a\"b", false)]
    [DataRow("1abc", false)]
    public void Options_ValidateIdentifiers(string name, bool valid)
    {
        var options = new PostgresWorkQueueOptions { ConnectionString = "Host=x", TasksTable = name };
        var results = new List<ValidationResult>();

        Assert.AreEqual(valid, Validator.TryValidateObject(options, new ValidationContext(options), results, true));
    }

    [TestMethod]
    public void OldestFirst_OrdersByDueTimeThenKey()
    {
        Assert.AreEqual("COALESCE(next_attempt_at, updated), item_key", new OldestFirstClaimOrder().OrderBy("any"));
    }

    [TestMethod]
    public async Task TryAddPostgresWorkQueue_RegistersTheQueueAndOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["WorkQueue:Postgres:ConnectionString"] = "Host=localhost",
            ["WorkQueue:Postgres:Schema"] = "jobs",
        }).Build();
        var services = new ServiceCollection();
        services.TryAddPostgresWorkQueue(configuration);
        await using var provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<PostgresWorkQueue>(provider.GetRequiredService<IWorkQueue>());
        Assert.AreEqual("jobs", provider.GetRequiredService<IOptions<PostgresWorkQueueOptions>>().Value.Schema);
        Assert.IsInstanceOfType<OldestFirstClaimOrder>(provider.GetRequiredService<IPostgresClaimOrder>());
    }
}
