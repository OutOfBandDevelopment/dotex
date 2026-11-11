using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Microsoft.Azure.AISearch.Tests;

[TestClass]
public class AzureAISearchMapperTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateIndex_SetsKeyVectorDimensionsAndProfile()
    {
        // Test
        var index = AzureAISearchMapper.CreateIndex("docs", 384);

        // Assert
        Assert.AreEqual("docs", index.Name);
        var vector = index.Fields.Single(f => f.Name == AzureAISearchGlobals.VectorField);
        Assert.AreEqual(384, vector.VectorSearchDimensions);
        Assert.AreEqual(AzureAISearchGlobals.ProfileName, vector.VectorSearchProfileName);
        Assert.IsTrue(index.Fields.Single(f => f.Name == AzureAISearchGlobals.IdField).IsKey);
        Assert.AreEqual(1, index.VectorSearch.Profiles.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToDocument_LaterMetadataWinsAndPayloadRoundTrips()
    {
        // Stage
        var metadata = new List<KeyValuePair<string, object>>
        {
            new("file", "a.txt"),
            new("page", 2),
            new("file", "b.txt"),
        };

        // Test
        var document = AzureAISearchMapper.ToDocument(new float[] { 1, 2 }, metadata);
        var result = AzureAISearchMapper.ToResult(document, 0.5);

        // Assert
        CollectionAssert.AreEqual(new float[] { 1, 2 }, (float[])document[AzureAISearchGlobals.VectorField]);
        Assert.AreEqual(0.5f, result.Score);
        Assert.AreEqual("b.txt", result.MetaData!["file"]);
        Assert.AreEqual(2L, result.MetaData["page"]);
        Assert.AreEqual((string)document[AzureAISearchGlobals.IdField], result.ItemId);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ReadPayload_HandlesEmptyAndMixedValues()
    {
        // Test
        var empty = AzureAISearchMapper.ReadPayload(null);
        var mixed = AzureAISearchMapper.ReadPayload("""{"s":"x","d":1.5,"b":true,"n":null,"l":["a"],"o":{"k":1}}""");

        // Assert
        Assert.AreEqual(0, empty.Count);
        Assert.AreEqual("x", mixed["s"]);
        Assert.AreEqual(1.5, mixed["d"]);
        Assert.AreEqual(true, mixed["b"]);
        Assert.IsFalse(mixed.ContainsKey("n"));
        CollectionAssert.AreEqual(new object[] { "a" }, (List<object?>)mixed["l"]);
        Assert.AreEqual("""{"k":1}""", mixed["o"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateNeighborOptions_QueriesTheVectorField()
    {
        // Test
        var options = AzureAISearchMapper.CreateNeighborOptions(new float[] { 1, 2, 3 }, 7);

        // Assert
        Assert.AreEqual(7, options.Size);
        var query = (VectorizedQuery)options.VectorSearch.Queries.Single();
        Assert.AreEqual(7, query.KNearestNeighborsCount);
        CollectionAssert.AreEqual(new[] { AzureAISearchGlobals.VectorField }, query.Fields.ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ToResult_MissingFields_GivesEmptyItem()
    {
        // Test
        var result = AzureAISearchMapper.ToResult(new SearchDocument(), null);

        // Assert
        Assert.AreEqual("", result.ItemId);
        Assert.AreEqual(0f, result.Score);
        Assert.AreEqual(0, result.MetaData!.Count);
    }
}
