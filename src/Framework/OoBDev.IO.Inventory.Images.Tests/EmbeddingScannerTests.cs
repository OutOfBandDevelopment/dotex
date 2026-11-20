using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OoBDev.IO.Inventory.Images.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class EmbeddingScannerTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task ScanAsync_ImagesWithSimilarVectors_AreGrouped()
    {
        // Stage
        var root = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            foreach (var name in new[] { "a.png", "b.png", "c.png" }) await File.WriteAllBytesAsync(Path.Combine(root, name), [(byte)name[0]], TestContext.CancellationToken);
            var generator = new Mock<IEmbeddingGenerator<DataContent, Embedding<float>>>();
            generator.Setup(g => g.GenerateAsync(It.IsAny<IEnumerable<DataContent>>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<DataContent> items, EmbeddingGenerationOptions? _, CancellationToken _) =>
                {
                    var first = items.First().Data.Span[0];
                    return new GeneratedEmbeddings<Embedding<float>>([new Embedding<float>(first == (byte)'c' ? new float[] { 0, 1 } : new float[] { 1, 0 })]);
                });

            // Test
            var (groups, skipped) = await new EmbeddingScanner(generator.Object).ScanAsync(root, 0.95f, TestContext.CancellationToken);
            TestContext.WriteLine($"{groups.Count} group(s), {skipped.Count} skipped");

            // Assert
            Assert.HasCount(1, groups);
            Assert.HasCount(2, groups[0]);
            Assert.IsEmpty(skipped);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
