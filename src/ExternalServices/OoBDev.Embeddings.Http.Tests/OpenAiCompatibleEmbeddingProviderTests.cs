using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Embeddings.Http.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class OpenAiCompatibleEmbeddingProviderTests
{
    private static OpenAiCompatibleEmbeddingProvider Build(StubHandler handler, int dimensions = 2, int batch = 32, string model = "m1")
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://embed.test/") };
        return new OpenAiCompatibleEmbeddingProvider(client, Options.Create(new HttpEmbeddingOptions { BaseUrl = "http://embed.test", Model = model, Dimensions = dimensions, BatchSize = batch }));
    }

    private static string Vectors(params int[] indexes) =>
        JsonSerializer.Serialize(new { data = indexes.Select(i => new { index = i, embedding = new[] { (float)i, 1f } }) });

    [TestMethod]
    public async Task GenerateEmbedding_Single_PostsModelAndInput()
    {
        var handler = new StubHandler((_, _) => (HttpStatusCode.OK, Vectors(0)));
        var provider = Build(handler);

        var vector = await provider.GenerateEmbeddingAsync("hello", null);

        Assert.AreEqual(2, vector.Length);
        Assert.AreEqual("http://embed.test/v1/embeddings", handler.Calls[0].Request.RequestUri!.ToString());
        using var body = JsonDocument.Parse(handler.Calls[0].Body);
        Assert.AreEqual("m1", body.RootElement.GetProperty("model").GetString());
        Assert.AreEqual("hello", body.RootElement.GetProperty("input")[0].GetString());
    }

    [TestMethod]
    public async Task GenerateEmbedding_ModelOverride_IsSent()
    {
        var handler = new StubHandler((_, _) => (HttpStatusCode.OK, Vectors(0)));

        await Build(handler).GenerateEmbeddingAsync("x", "other");

        StringAssert.Contains(handler.Calls[0].Body, "\"model\":\"other\"");
    }

    [TestMethod]
    public async Task GenerateEmbeddings_OutOfOrderResponse_ComesBackInRequestOrder()
    {
        var handler = new StubHandler((_, _) => (HttpStatusCode.OK, Vectors(2, 0, 1)));

        var vectors = await Build(handler).GenerateEmbeddingsAsync(["a", "b", "c"]);

        CollectionAssert.AreEqual(new[] { 0f, 1f, 2f }, vectors.Select(v => v.Span[0]).ToArray());
    }

    [TestMethod]
    public async Task GenerateEmbeddings_MoreThanBatchSize_SplitsIntoRequests()
    {
        var handler = new StubHandler((_, body) => (HttpStatusCode.OK, Vectors(Enumerable.Range(0, JsonDocument.Parse(body).RootElement.GetProperty("input").GetArrayLength()).ToArray())));

        var vectors = await Build(handler, batch: 2).GenerateEmbeddingsAsync(["a", "b", "c", "d", "e"]);

        Assert.AreEqual(5, vectors.Count);
        Assert.AreEqual(3, handler.Calls.Count);
    }

    [TestMethod]
    public async Task GenerateEmbeddings_NoInputs_SendsNothing()
    {
        var handler = new StubHandler((_, _) => (HttpStatusCode.OK, Vectors()));

        var vectors = await Build(handler).GenerateEmbeddingsAsync([]);

        Assert.AreEqual(0, vectors.Count);
        Assert.AreEqual(0, handler.Calls.Count);
    }

    [TestMethod]
    public async Task GenerateEmbedding_ServerError_ThrowsWithoutInputOrBody()
    {
        var handler = new StubHandler((_, _) => (HttpStatusCode.InternalServerError, "secret-body"));

        var ex = await Assert.ThrowsExactlyAsync<EmbeddingServerException>(() => Build(handler).GenerateEmbeddingAsync("private-text", null));

        Assert.AreEqual(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.DoesNotContain("private-text", ex.Message);
        Assert.DoesNotContain("secret-body", ex.Message);
    }

    [TestMethod]
    public async Task GenerateEmbeddings_WrongCount_Throws()
    {
        var handler = new StubHandler((_, _) => (HttpStatusCode.OK, Vectors(0)));

        await Assert.ThrowsExactlyAsync<EmbeddingServerException>(() => Build(handler).GenerateEmbeddingsAsync(["a", "b"]));
    }

    [TestMethod]
    public async Task GenerateEmbedding_WrongDimension_Throws()
    {
        var handler = new StubHandler((_, _) => (HttpStatusCode.OK, Vectors(0)));

        await Assert.ThrowsExactlyAsync<EmbeddingServerException>(() => Build(handler, dimensions: 3).GenerateEmbeddingAsync("a", null));
    }

    [TestMethod]
    public void Length_IsConfiguredDimensions() => Assert.AreEqual(7, Build(new StubHandler((_, _) => (HttpStatusCode.OK, "{}")), dimensions: 7).Length);
}
