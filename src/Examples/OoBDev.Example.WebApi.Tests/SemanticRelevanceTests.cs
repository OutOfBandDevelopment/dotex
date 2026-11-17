using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.SBert;
using OoBDev.SBert.AllMiniLmL6V2;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading.Tasks;

namespace OoBDev.Example.WebApi.Tests;

/// <summary>
/// A judged query set for the semantic and hybrid search: paraphrases that share no keyword with the passage they should find, so
/// keyword search cannot answer them. Runs the in-process embedding model (downloaded on first use), so it is an Integration test.
/// </summary>
[TestClass]
public class SemanticRelevanceTests
{
    private static readonly (string Query, string Expected)[] Paraphrases =
    [
        ("how do I log in without keeping user credentials", "jwt-bearer"),
        ("where are my uploaded documents kept", "blob-storage"),
        ("find items that mean the same thing", "vector-embed"),
        ("who is allowed to call which endpoint", "rights-roles"),
        ("monitoring data sent to a collector", "telemetry"),
    ];

    private static async Task<(IList<SearchHitModel> Semantic, IList<SearchHitModel> Hybrid)> RunAsync(IEmbeddingProvider embedding, string query)
    {
        var corpus = SearchRelevanceTests.Corpus;
        var question = (await embedding.GenerateEmbeddingAsync(query, null, default)).ToArray();
        var scored = new List<SearchHitModel>();
        foreach (var passage in corpus)
        {
            var vector = (await embedding.GenerateEmbeddingAsync(passage.Text, null, default)).ToArray();
            scored.Add(new SearchHitModel { Id = passage.Id, Text = passage.Text, Score = TensorPrimitives.CosineSimilarity(question, vector) });
        }

        var semantic = scored.OrderByDescending(h => h.Score).ToList();
        return (semantic, SearchScoring.Fuse(SearchScoring.Lexical(query, corpus), semantic));
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Semantic_Paraphrases_FindTheExpectedPassageInTheTopThree()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddAllMiniLmL6V2Services(new ConfigurationBuilder().Build(), SBertGlobals.DefaultSection);
        await using var provider = services.BuildServiceProvider();
        var embedding = provider.GetRequiredKeyedService<IEmbeddingProvider>(SBertGlobals.AllMiniLmL6V2Key);

        var semanticHits = 0;
        var hybridHits = 0;
        foreach (var (query, expected) in Paraphrases)
        {
            var (semantic, hybrid) = await RunAsync(embedding, query);
            semanticHits += semantic.Take(3).Any(h => h.Id == expected) ? 1 : 0;
            hybridHits += hybrid.Take(3).Any(h => h.Id == expected) ? 1 : 0;
        }

        Assert.IsGreaterThanOrEqualTo(4, semanticHits, "semantic hit rate at 3 over 5 paraphrases");
        Assert.IsGreaterThanOrEqualTo(4, hybridHits, "hybrid hit rate at 3 over 5 paraphrases");
    }
}
