using Microsoft.Extensions.Configuration;
using OoBDev.TestUtilities.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo4j.Driver;
using OoBDev.Search.Graph;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Neo4j.Tests;

[TestClass]
public class Neo4jGraphStoreTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Person")]
    [DataRow("_x1")]
    public void Name_ValidIdentifier_IsReturned(string value) =>
        Assert.AreEqual(value, Neo4jGraphStore.Name(value));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Person) DETACH DELETE n //")]
    [DataRow("1abc")]
    [DataRow("")]
    [DataRow("a-b")]
    public void Name_InvalidIdentifier_Throws(string value) =>
        Assert.ThrowsExactly<ArgumentException>(() => Neo4jGraphStore.Name(value));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddNeo4jServices_RegistersGraphStore()
    {
        // Stage
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Neo4j:Uri"] = "bolt://localhost:7687",
            ["Neo4j:User"] = "neo4j",
            ["Neo4j:Password"] = "x",
        }).Build();
        var services = new ServiceCollection();

        // Test
        services.TryAddNeo4jServices(configuration, "Neo4j");
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<Neo4jGraphStore>(provider.GetRequiredService<IGraphStore>());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Options_MissingUri_FailsValidation()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Neo4j:User"] = "neo4j",
            ["Neo4j:Password"] = "x",
        }).Build();
        var services = new ServiceCollection();
        services.TryAddNeo4jServices(configuration, "Neo4j");
        using var provider = services.BuildServiceProvider();

        Assert.ThrowsExactly<OptionsValidationException>(() => _ = provider.GetRequiredService<IOptions<Neo4jOptions>>().Value);
    }

    [IntegrationTestMethod("oobd-test-neo4j")]
    [TestCategory(TestCategories.Integration)]
    public async Task Graph_RoundTrip_AgainstRealNeo4j()
    {
        // Stage
        var uri = TestContext.GetPropertyOrDefault("NEO4J_URI", "bolt://localhost:7687");
        var user = TestContext.GetPropertyOrDefault("NEO4J_USER", "neo4j");
        var password = TestContext.GetPropertyOrDefault("NEO4J_PASSWORD", "oobdev-test-pass");
        await using var driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        var store = new Neo4jGraphStore(driver, Options.Create(new Neo4jOptions { Uri = uri, User = user, Password = password }));
        var run = Guid.NewGuid().ToString("N");
        var a = new GraphNode($"a-{run}", "TestNode", new Dictionary<string, object?> { ["name"] = "A" });
        var b = new GraphNode($"b-{run}", "TestNode");
        var c = new GraphNode($"c-{run}", "TestNode");
        var d = new GraphNode($"d-{run}", "TestNode");
        try
        {
            // Test
            foreach (var node in new[] { a, b, c, d })
                await store.UpsertNodeAsync(node);
            await store.ConnectAsync(a, "KNOWS", b);
            await store.ConnectAsync(b, "KNOWS", c);
            await store.ConnectAsync(a, "KNOWS", b);

            var near = await store.NeighborsAsync(a, 1).ToListAsync();
            var far = await store.NeighborsAsync(a, 2, "KNOWS").ToListAsync();
            var path = await store.ShortestPathAsync(a, c);
            var none = await store.ShortestPathAsync(a, d);

            // Assert
            CollectionAssert.AreEqual(new[] { b.Id }, near.Select(n => n.Id).ToArray());
            CollectionAssert.AreEqual(new[] { b.Id, c.Id }, far.Select(n => n.Id).ToArray());
            CollectionAssert.AreEqual(new[] { a.Id, b.Id, c.Id }, path.Select(n => n.Id).ToArray());
            Assert.AreEqual(0, none.Count);
            Assert.AreEqual("A", path[0].Properties["name"]);
        }
        finally
        {
            foreach (var node in new[] { a, b, c, d })
                await store.DeleteNodeAsync(node);
        }
    }
}
