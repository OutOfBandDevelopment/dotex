using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search;
using OoBDev.TestUtilities;

namespace OoBDev.Data.Vectors.Postgres.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class PgVectorUnitTests
{
    private static PgVectorSql Sql(VectorMetric metric = VectorMetric.Cosine, bool half = false) =>
        new(new PgVectorOptions { Schema = "s", Table = "t", Dimensions = 4, Metric = metric, UseHalfVector = half });

    [TestMethod]
    public void Operator_FollowsMetric()
    {
        Assert.AreEqual("<=>", Sql().Operator);
        Assert.AreEqual("<->", Sql(VectorMetric.L2).Operator);
    }

    [TestMethod]
    public void OperatorClass_FollowsMetricAndHalf()
    {
        Assert.AreEqual("vector_cosine_ops", Sql().OperatorClass);
        Assert.AreEqual("vector_l2_ops", Sql(VectorMetric.L2).OperatorClass);
        Assert.AreEqual("halfvec_cosine_ops", Sql(half: true).OperatorClass);
        Assert.AreEqual("halfvec_l2_ops", Sql(VectorMetric.L2, true).OperatorClass);
    }

    [TestMethod]
    public void CreateTable_UsesVectorOrHalfvecWithDimensions()
    {
        StringAssert.Contains(Sql().CreateTable, "embedding vector(4)");
        StringAssert.Contains(Sql(half: true).CreateTable, "embedding halfvec(4)");
    }

    [TestMethod]
    public void CreateHnswIndex_IsSeparateFromCreateTable()
    {
        Assert.DoesNotContain("hnsw", Sql().CreateTable);
        StringAssert.Contains(Sql().CreateHnswIndex, "USING hnsw (embedding vector_cosine_ops)");
    }

    [TestMethod]
    public void Statements_QuoteIdentifiersAndBindValues()
    {
        var sql = Sql();

        Assert.AreEqual("\"s\".\"t\"", sql.Table);
        StringAssert.Contains(sql.Insert, "@embedding");
        StringAssert.Contains(sql.Neighbours, "LIMIT @limit");
        StringAssert.Contains(sql.NeighboursBestPerGroup, "metadata ->> @group");
    }

    [TestMethod]
    public void Score_CosineIsSimilarityAndL2IsInverse()
    {
        Assert.AreEqual(0.75, Sql().Score(0.25), 1e-9);
        Assert.AreEqual(0.5, Sql(VectorMetric.L2).Score(1), 1e-9);
    }

    [TestMethod]
    public void Options_BadIdentifier_FailsValidation()
    {
        var options = new PgVectorOptions { ConnectionString = "x", Dimensions = 3, Table = "a\"; DROP TABLE x;--" };

        Assert.IsFalse(Validator.TryValidateObject(options, new ValidationContext(options), [], true));
    }

    [TestMethod]
    public async Task TryAddPgVectorStore_ResolvesPlainAndKeyedStore()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Vectors:Postgres:ConnectionString"] = "Host=localhost;Database=oobdev_test",
            ["Vectors:Postgres:Dimensions"] = "4",
        }).Build();
        var services = new ServiceCollection();
        services.TryAddPgVectorStore(config);
        await using var provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<PgVectorStore>(provider.GetRequiredService<IVectorStore>());
        Assert.AreSame(provider.GetRequiredService<IVectorStore>(), provider.GetRequiredKeyedService<IVectorStore>(PgVectorGlobals.ProviderKey));
    }
}
