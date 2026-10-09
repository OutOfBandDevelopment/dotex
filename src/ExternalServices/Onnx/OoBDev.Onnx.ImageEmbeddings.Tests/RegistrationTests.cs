using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Onnx.ImageEmbeddings.Skia;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using OoBDev.Vision.ClipVitB32;
using OoBDev.Vision.Dinov2Small;
using OoBDev.Vision.VitBasePatch16;
using System.Threading.Tasks;
using Clip = OoBDev.Vision.ClipVitB32;
using Dino = OoBDev.Vision.Dinov2Small;
using Vit = OoBDev.Vision.VitBasePatch16;

namespace OoBDev.Onnx.ImageEmbeddings.Tests;

/// <summary>Dependency injection registration of the presets; models come from the shared Hugging Face cache.</summary>
[TestClass]
public class RegistrationTests
{
    private static DataContent Image() =>
        new(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "TestData", "images", "scene-224.png")), "image/png");

    private static ServiceProvider Build()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new global::System.Collections.Generic.Dictionary<string, string?>
        {
            ["Dinov2Small:MaxBatchSize"] = "2",
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddSkiaImageDecoder();
        services.TryAddSkiaImageDecoder(); // second call must not add a second decoder
        services.TryAddDinov2SmallServices(configuration, Dino.VisionGlobals.DefaultSection);
        services.TryAddVitBasePatch16Services(configuration, Vit.VisionGlobals.DefaultSection);
        services.TryAddClipVitB32Services(configuration, Clip.VisionGlobals.DefaultSection);
        return services.BuildServiceProvider();
    }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task AllPresets_ResolveByKeyAndWork()
    {
        using var provider = Build();
        Assert.AreEqual(1, provider.GetServices<IImageDecoder>().Count());

        try
        {
            var dino = provider.GetRequiredKeyedService<IEmbeddingGenerator<DataContent, Embedding<float>>>(Dino.VisionGlobals.Dinov2SmallKey);
            Assert.AreEqual(384, (await dino.GenerateAsync([Image()]))[0].Vector.Length);
            Assert.IsNotNull(dino.GetService<EmbeddingGeneratorMetadata>());
            Assert.IsNull(dino.GetService(typeof(EmbeddingGeneratorMetadata), "other-key"));

            var vit = provider.GetRequiredKeyedService<IImageClassifier>(Vit.VisionGlobals.VitBasePatch16Key);
            Assert.HasCount(3, await vit.ClassifyAsync(Image(), 3));

            var clipImages = provider.GetRequiredKeyedService<IEmbeddingGenerator<DataContent, Embedding<float>>>(Clip.VisionGlobals.ClipVitB32Key);
            var clipTexts = provider.GetRequiredKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(Clip.VisionGlobals.ClipVitB32Key);
            Assert.AreEqual(512, (await clipImages.GenerateAsync([Image()]))[0].Vector.Length);
            Assert.AreEqual(512, (await clipTexts.GenerateAsync(["a photo"]))[0].Vector.Length);
            Assert.IsNotNull(clipTexts.GetService<EmbeddingGeneratorMetadata>());

            var zeroShot = provider.GetRequiredKeyedService<Clip.ZeroShotImageClassifier>(Clip.VisionGlobals.ClipVitB32Key);
            Assert.HasCount(2, await zeroShot.ClassifyAsync(Image(), ["a landscape", "random noise"]));
        }
        catch (global::System.Net.Http.HttpRequestException ex)
        {
            Assert.Inconclusive("A model could not be downloaded: " + ex.Message);
        }
    }
}
