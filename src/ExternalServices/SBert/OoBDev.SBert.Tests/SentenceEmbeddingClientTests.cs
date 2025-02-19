using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.SBert.Tests;

[TestClass]
public class SentenceEmbeddingClientTests
{
    public required TestContext TestContext { get; set; }

    private ISentenceEmbeddingClient BuildClient(string url)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "SentenceEmbeddingOptions:Url", url },
            })
            .Build();
        var services = new ServiceCollection()
            .TryAddSbertServices(configuration, nameof(SentenceEmbeddingOptions))
            .BuildServiceProvider();
        var client = services.GetRequiredService<ISentenceEmbeddingClient>();
        return client;
    }

    [DataTestMethod, TestCategory(TestCategories.DevLocal)]
    [DataRow("http://127.0.0.1:5080", "Hello World!")]
    public async Task GetEmbeddingAsyncTest(string url, string message)
    {
        var client = BuildClient(url);
        var embedding = await client.GetEmbeddingAsync(message);
        TestContext.WriteLine(string.Join(';', embedding));
    }

    [DataTestMethod, TestCategory(TestCategories.DevLocal)]
    [DataRow("http://127.0.0.1:5080")]
    public async Task GetAllTest(string url)
    {
        var client = BuildClient(url);

        var resource = GetType().Assembly.GetManifestResourceNames().FirstOrDefault(l => l.EndsWith(".Sentences.txt"))
            ?? throw new ApplicationException("missing .sentences.txt resource");
        using var stream = GetType().Assembly.GetManifestResourceStream(resource)
            ?? throw new ApplicationException("missing .sentences.txt resource");
        using var reader = new StreamReader(stream);

        var dict = new Dictionary<string, ReadOnlyMemory<double>>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(line?.Trim())) continue;

                var parts = line.Split([','], 2);
                var section = parts[0].Trim();
                var segment = parts[1].Trim();

                TestContext.WriteLine(line);

                //var embedding = await client.GetEmbeddingAsync(segment);
                var embedding = await client.GetEmbeddingDoubleAsync(segment);

                dict.TryAdd(segment, embedding);
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"ERROR: \"{line}\" -> {ex.Message}");
            }
        }

        var each = from a in dict
                   from b in dict
                   let dotproduct = a.Value.ToArray().Zip(b.Value.ToArray()).Select(i => i.First * i.Second).Sum()
                   let vma = Math.Sqrt(a.Value.ToArray().Select(i => i * i).Sum())
                   let vmb = Math.Sqrt(b.Value.ToArray().Select(i => i * i).Sum())
                   select new
                   {
                       A = a.Key,
                       B = b.Key,
                       Dotproduct = dotproduct,
                       ConsineSimilarity = dotproduct / (vma * vmb),
                       EuclideanDistance = Math.Sqrt(a.Value.ToArray().Zip(b.Value.ToArray()).Select(i => Math.Pow(i.First - i.Second, 2)).Sum()),
                       ManhattanDistance = a.Value.ToArray().Zip(b.Value.ToArray()).Select(i => Math.Abs(i.First - i.Second)).Sum(),
                       VectorMagnitudeA = vma,
                       VectorMagnitudeB = vmb,
                   };

        TestContext.AddResult(
            each.Select(i => string.Join('\t',
            i.A,
            i.B,
            i.Dotproduct,
            i.ConsineSimilarity,
            i.EuclideanDistance,
            i.ManhattanDistance,
            i.VectorMagnitudeA,
            i.VectorMagnitudeB
            )).ToArray(), fileName: "Results.txt"
            );

        TestContext.AddResult(
            dict.Select(i => string.Join('\t', i.Key, Convert.ToBase64String(i.Value.ToArray().Select(i => BitConverter.GetBytes(i)).SelectMany(i => i).ToArray()))),
            fileName: "Embeddings.txt"
            );

        TestContext.AddResult(
            dict.Select(i => string.Join('\t', new object[] { i.Key }.Concat(i.Value.ToArray().Select(i => (object)i)))),
            fileName: "EmbeddingsD.txt"
            );
    }
}
