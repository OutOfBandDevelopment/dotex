using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace OoBDev.Data.DesiredState.Tests;

[TestClass]
public class ManifestCompilerTests
{
    public TestContext TestContext { get; set; } = null!;

    private const string Json = """{ "extensions": ["pgcrypto"], "queues": ["orders"], "streams": ["order_events"] }""";

    [TestMethod]
    [TestCategory("Unit")]
    public void Compile_Postgres_ProducesOrderedIdempotentScripts()
    {
        var schema = ManifestCompiler.Compile(FeatureManifest.Parse(Json), "postgres");
        TestContext.WriteLine(schema.Sql);

        CollectionAssert.AreEqual(new[] { "001_extensions.sql", "020_queues.sql", "030_streams.sql" }, schema.Scripts.Select(s => s.Name).ToArray());
        StringAssert.Contains(schema.Scripts[0].Text, "CREATE EXTENSION IF NOT EXISTS pgcrypto;");
        StringAssert.Contains(schema.Scripts[1].Text, "CREATE TABLE IF NOT EXISTS queue_orders");
        StringAssert.Contains(schema.Scripts[2].Text, "CREATE TABLE IF NOT EXISTS stream_order_events");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Compile_SqlServerAndSqlite_GuardEachCreate()
    {
        var manifest = FeatureManifest.Parse("""{ "queues": ["orders"], "streams": ["e"] }""");

        var sqlServer = ManifestCompiler.Compile(manifest, "SqlServer");
        var sqlite = ManifestCompiler.Compile(manifest, "sqlite");

        StringAssert.Contains(sqlServer.Scripts[0].Text, "IF OBJECT_ID(N'queue_orders', N'U') IS NULL CREATE TABLE");
        StringAssert.Contains(sqlite.Scripts[0].Text, "CREATE TABLE IF NOT EXISTS queue_orders");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Compile_EmptyManifest_ProducesNoScripts()
    {
        Assert.AreEqual(0, ManifestCompiler.Compile(FeatureManifest.Parse("{}"), "postgres").Scripts.Count);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Compile_InvalidName_Throws()
    {
        var manifest = FeatureManifest.Parse("""{ "queues": ["x; DROP TABLE users"] }""");

        Assert.ThrowsExactly<ArgumentException>(() => ManifestCompiler.Compile(manifest, "postgres"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Compile_UnsupportedEngineOrExtensionsOffPostgres_Throws()
    {
        Assert.ThrowsExactly<NotSupportedException>(() => ManifestCompiler.Compile(FeatureManifest.Parse(Json), "oracle"));
        Assert.ThrowsExactly<NotSupportedException>(() => ManifestCompiler.Compile(FeatureManifest.Parse(Json), "sqlite"));
    }
}
