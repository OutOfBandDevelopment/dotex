using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.SBert.Tests;

[TestClass]
public class SentenceEmbeddingClientRequestTests
{
    private sealed class RecordingHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"embedding\":[0.5,1.5]}"),
            });
        }
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task GetEmbeddingAsync_SpecialCharacters_EscapesQuery()
    {
        // Stage
        var handler = new RecordingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("http://sbert.test") };
        var client = new SentenceEmbeddingClient(http);

        // Test
        var embedding = await client.GetEmbeddingAsync("a&b=c d#e");

        // Assert
        Assert.AreEqual("?query=a%26b%3Dc%20d%23e", handler.RequestUri!.Query);
        Assert.AreEqual(2, embedding.Length);
        Assert.AreEqual(1.5f, embedding.Span[1]);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task GetEmbeddingDoubleAsync_SpecialCharacters_EscapesQuery()
    {
        // Stage
        var handler = new RecordingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("http://sbert.test") };
        var client = new SentenceEmbeddingClient(http);

        // Test
        var embedding = await client.GetEmbeddingDoubleAsync("x y&z");

        // Assert
        Assert.AreEqual("?query=x%20y%26z", handler.RequestUri!.Query);
        Assert.AreEqual(0.5d, embedding.Span[0]);
    }
}
