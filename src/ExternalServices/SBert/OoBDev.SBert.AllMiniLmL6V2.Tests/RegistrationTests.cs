using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics.Tensors;
using System.Threading.Tasks;

namespace OoBDev.SBert.AllMiniLmL6V2.Tests;

[TestClass]
public class RegistrationTests
{
    private static string? FindModel()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ExternalServices", "SBert", "OoBDev.SBert.AllMiniLML6v2Sharp", "model");
            if (File.Exists(Path.Combine(candidate, "model.onnx"))) return candidate;
        }
        return null;
    }

    private static ServiceProvider Build(string? modelPath)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AllMiniLmL6V2:ModelPath"] = modelPath })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddAllMiniLmL6V2Services(configuration, SBertGlobals.DefaultSection);
        return services.BuildServiceProvider();
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Keys_AreKebabCaseWithLegacyAlias()
    {
        Assert.AreEqual("all-minilm-l6-v2", SBertGlobals.AllMiniLmL6V2Key);
        Assert.AreEqual("ALLMINILM", SBertGlobals.LegacyKey);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Provider_ResolvesByDefaultAndKeys_AndEmbeds()
    {
        var model = FindModel() ?? throw new AssertInconclusiveException("The all-MiniLM-L6-v2 model files are not checked out.");
        await using var provider = Build(model);

        var byDefault = provider.GetRequiredService<IEmbeddingProvider>();
        var byKey = provider.GetRequiredKeyedService<IEmbeddingProvider>(SBertGlobals.AllMiniLmL6V2Key);
        var legacy = provider.GetRequiredKeyedService<IEmbeddingProvider>(SBertGlobals.LegacyKey);

        Assert.AreEqual(384, byDefault.Length);
        var a = await byDefault.GenerateEmbeddingAsync("hello world", null, default);
        var b = await byKey.GenerateEmbeddingAsync("hello world", null, default);
        var c = await legacy.GenerateEmbeddingAsync("hello world", null, default);
        Assert.IsGreaterThan(0.9999f, TensorPrimitives.CosineSimilarity(a.Span, b.Span));
        Assert.IsGreaterThan(0.9999f, TensorPrimitives.CosineSimilarity(a.Span, c.Span));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task Provider_BlankContent_ReturnsZeroVectorOfFullLength()
    {
        var model = FindModel() ?? throw new AssertInconclusiveException("The all-MiniLM-L6-v2 model files are not checked out.");
        await using var provider = Build(model);

        var result = await provider.GetRequiredService<IEmbeddingProvider>().GenerateEmbeddingAsync(" ", null, default);

        Assert.AreEqual(384, result.Length);
        Assert.AreEqual(0f, TensorPrimitives.Norm(result.Span));
    }
}
