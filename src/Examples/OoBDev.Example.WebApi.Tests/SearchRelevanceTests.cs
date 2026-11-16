using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

/// <summary>
/// A judged query set for the example keyword search: each query names the passage a reader would expect first. The set guards the
/// scoring against regressions and gives a baseline (mean reciprocal rank, hit rate at 3) that a hybrid or vector run is compared with.
/// </summary>
[TestClass]
public class SearchRelevanceTests
{
    private static readonly RagPassage[] Corpus =
    [
        new() { Id = "queue-rabbit", Text = "Messages are published to a RabbitMQ queue by the RabbitMQ adapter and received by a consumer." },
        new() { Id = "queue-sqs", Text = "The SQS adapter sends and receives messages on an Amazon queue with long polling." },
        new() { Id = "template-files", Text = "Templates are files named after the template and culture, for example welcome.es-ES." },
        new() { Id = "rights-roles", Text = "Application rights guard each endpoint and roles are translated to rights in middleware." },
        new() { Id = "jwt-bearer", Text = "The API validates a JWT bearer token issued by the identity provider and never stores passwords." },
        new() { Id = "blob-storage", Text = "Blob containers store files in Azure storage, Amazon S3 or the local disk behind one interface." },
        new() { Id = "vector-embed", Text = "Text is embedded into a vector and stored so the nearest neighbours answer a similarity query." },
        new() { Id = "telemetry", Text = "OpenTelemetry exports traces, logs and metrics to an OTLP collector when configured." },
    ];

    private static readonly (string Query, string Expected)[] Judged =
    [
        ("rabbitmq consumer", "queue-rabbit"),
        ("amazon long polling", "queue-sqs"),
        ("template culture welcome", "template-files"),
        ("roles translated to rights", "rights-roles"),
        ("jwt bearer token passwords", "jwt-bearer"),
        ("s3 azure local disk files", "blob-storage"),
        ("nearest neighbours similarity", "vector-embed"),
        ("otlp traces metrics", "telemetry"),
    ];

    private static (double MeanReciprocalRank, double HitRateAt3) Evaluate()
    {
        var reciprocal = new List<double>();
        var hits = 0;
        foreach (var (query, expected) in Judged)
        {
            var ranked = SearchScoring.Lexical(query, Corpus).Select(h => h.Id).ToList();
            var rank = ranked.IndexOf(expected) + 1;
            reciprocal.Add(rank == 0 ? 0 : 1.0 / rank);
            if (rank is >= 1 and <= 3)
            {
                hits++;
            }
        }

        return (reciprocal.Average(), hits / (double)Judged.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Lexical_JudgedQueries_ReturnTheExpectedPassageFirst()
    {
        foreach (var (query, expected) in Judged)
        {
            var top = SearchScoring.Lexical(query, Corpus).FirstOrDefault();

            Assert.AreEqual(expected, top?.Id, $"query \"{query}\"");
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Lexical_JudgedQueries_MeetTheBaseline()
    {
        var (mrr, hitRate) = Evaluate();

        Assert.IsGreaterThanOrEqualTo(0.9, mrr);
        Assert.AreEqual(1.0, hitRate, 1e-12);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Lexical_QueryWithNoJudgedAnswer_ReturnsNothing() =>
        Assert.IsEmpty(SearchScoring.Lexical("zeppelin", Corpus));
}
