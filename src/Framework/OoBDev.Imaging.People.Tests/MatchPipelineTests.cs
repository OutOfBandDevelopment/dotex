using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Imaging.People.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class MatchPipelineTests
{
    public required TestContext TestContext { get; set; }

    private static readonly byte[] Image = [1, 2, 3];

    private sealed class FakeDetector(params DetectedItem[] found) : IFaceDetector, IPetDetector
    {
        public int Calls { get; private set; }

        public ValueTask<IReadOnlyList<DetectedItem>> DetectAsync(byte[] image, CancellationToken cancellationToken = default)
        {
            Calls++;
            return ValueTask.FromResult<IReadOnlyList<DetectedItem>>(found);
        }
    }

    private sealed class FakeEmbedder(Func<DetectedItem, float[]> embed) : IFaceEmbedder, IPetEmbedder
    {
        public int Calls { get; private set; }

        public ValueTask<float[]> EmbedAsync(byte[] image, DetectedItem item, CancellationToken cancellationToken = default)
        {
            Calls++;
            return ValueTask.FromResult(embed(item));
        }
    }

    private static DetectedItem Face(float x) => new(MatchKind.Face, new ImageBox(x, 0, 10, 10), 0.9);

    private static DetectedItem Pet(float x, string species) => new(MatchKind.Pet, new ImageBox(x, 0, 10, 10), 0.8, species);

    private static float[] V(params float[] v) => v;

    [TestMethod]
    public async Task Run_AllStages_DetectsEmbedsAndJoinsTheKnownCluster()
    {
        var detector = new FakeDetector(Face(1));
        var embedder = new FakeEmbedder(_ => V(1, 0.05f));
        var pipeline = new MatchPipeline(detector, embedder);
        var request = new MatchRequest
        {
            Image = Image,
            Kinds = MatchKinds.Faces,
            Candidates = [new ClusterItem(10, V(1, 0), 7), new ClusterItem(11, V(0, 1), 8)],
        };

        var response = await pipeline.RunAsync(request, TestContext.CancellationToken);
        var item = response.Items.Single();
        TestContext.WriteLine($"cluster={item.Cluster} similarity={item.Similarity:F3} why={item.Why} nearest={string.Join(",", item.Nearest.Select(n => n.Id))}");

        Assert.AreEqual(7, item.Cluster);
        Assert.AreEqual("threshold met", item.Why);
        Assert.AreEqual(10, item.Nearest[0].Id);
        Assert.AreEqual(2, item.Nearest.Count);
        Assert.IsNull(item.Embedding);
        Assert.AreEqual(MatchStages.All, response.StagesRun);
    }

    [TestMethod]
    public async Task Run_IncludeEmbeddings_KeepsTheVector()
    {
        var pipeline = new MatchPipeline(new FakeDetector(Face(1)), new FakeEmbedder(_ => V(1, 0)));

        var response = await pipeline.RunAsync(new MatchRequest { Image = Image, Kinds = MatchKinds.Faces, IncludeEmbeddings = true }, TestContext.CancellationToken);

        CollectionAssert.AreEqual(V(1, 0), response.Items.Single().Embedding);
    }

    [TestMethod]
    public async Task Run_NothingKnown_SeedsNewClustersAndCountsThem()
    {
        var detector = new FakeDetector(Face(1), Face(20));
        var embedder = new FakeEmbedder(i => i.Box.X < 10 ? V(1, 0) : V(0, 1));
        var pipeline = new MatchPipeline(detector, embedder);

        var response = await pipeline.RunAsync(new MatchRequest { Image = Image, Kinds = MatchKinds.Faces }, TestContext.CancellationToken);
        TestContext.WriteLine($"clusters={string.Join(",", response.Items.Select(i => i.Cluster))} new={response.NewFaceClusters}");

        Assert.AreEqual(2, response.NewFaceClusters);
        CollectionAssert.AreEqual(new int?[] { -1, -2 }, response.Items.Select(i => i.Cluster).ToArray());
        StringAssert.Contains(response.Items[0].Why, "seeded");
    }

    [TestMethod]
    public async Task Run_Pets_MatchOnlyTheirOwnSpecies()
    {
        var detector = new FakeDetector(Pet(1, "dog"));
        var embedder = new FakeEmbedder(_ => V(1, 0));
        var pipeline = new MatchPipeline(null, null, detector, embedder);
        var request = new MatchRequest
        {
            Image = Image,
            Kinds = MatchKinds.Pets,
            Candidates = [new ClusterItem(10, V(1, 0), 3, "cat"), new ClusterItem(11, V(1, 0), 4, "dog")],
        };

        var response = await pipeline.RunAsync(request, TestContext.CancellationToken);
        var item = response.Items.Single();
        TestContext.WriteLine($"cluster={item.Cluster} nearest={string.Join(",", item.Nearest.Select(n => n.Id))}");

        Assert.AreEqual(4, item.Cluster);
        Assert.AreEqual(11, item.Nearest.Single().Id);
    }

    [TestMethod]
    public async Task Run_SpeciesFilter_DropsOtherSpecies()
    {
        var detector = new FakeDetector(Pet(1, "dog"), Pet(20, "cat"));
        var pipeline = new MatchPipeline(null, null, detector);

        var response = await pipeline.RunAsync(
            new MatchRequest { Image = Image, Kinds = MatchKinds.Pets, Stages = MatchStages.Detect, Species = new HashSet<string> { "cat" } },
            TestContext.CancellationToken);

        Assert.AreEqual("cat", response.Items.Single().Detection.Species);
    }

    [TestMethod]
    public async Task Run_DetectOnly_ThenResumeWithEmbedAndCluster()
    {
        var detector = new FakeDetector(Face(1));
        var embedder = new FakeEmbedder(_ => V(1, 0));
        var first = await new MatchPipeline(detector).RunAsync(
            new MatchRequest { Image = Image, Kinds = MatchKinds.Faces, Stages = MatchStages.Detect },
            TestContext.CancellationToken);

        var second = await new MatchPipeline(null, embedder).RunAsync(
            new MatchRequest { Image = Image, Stages = MatchStages.Embed | MatchStages.Cluster, Items = first.Items, Candidates = [new ClusterItem(5, V(1, 0), 2)] },
            TestContext.CancellationToken);
        TestContext.WriteLine($"first stages={first.StagesRun} second stages={second.StagesRun} cluster={second.Items.Single().Cluster}");

        Assert.IsNull(first.Items.Single().Cluster);
        Assert.AreEqual(1, detector.Calls);
        Assert.AreEqual(2, second.Items.Single().Cluster);
    }

    [TestMethod]
    public async Task Run_ClusterOnly_UsesTheEmbeddingsOfTheItemsAndNeedsNoModels()
    {
        var items = new[] { new MatchedItem { Detection = Face(1), Embedding = V(0, 1) } };

        var response = await new MatchPipeline().RunAsync(
            new MatchRequest { Stages = MatchStages.Cluster, Items = items, Candidates = [new ClusterItem(1, V(0, 1), 9)] },
            TestContext.CancellationToken);

        Assert.AreEqual(9, response.Items.Single().Cluster);
    }

    [TestMethod]
    public async Task Run_EmbeddedItemIsNotEmbeddedAgain()
    {
        var embedder = new FakeEmbedder(_ => V(0, 1));
        var items = new[] { new MatchedItem { Detection = Face(1), Embedding = V(0, 1) } };

        await new MatchPipeline(null, embedder).RunAsync(
            new MatchRequest { Image = Image, Stages = MatchStages.Embed, Items = items },
            TestContext.CancellationToken);

        Assert.AreEqual(0, embedder.Calls);
    }

    [TestMethod]
    public async Task Run_ConfirmedAssignmentsOfCandidatesNeverChange()
    {
        var candidates = new[] { new ClusterItem(1, V(1, 0), 4, Confirmed: true), new ClusterItem(2, V(1, 0), 4) };
        var items = new[] { new MatchedItem { Detection = Face(1), Embedding = V(0, 1) } };

        var response = await new MatchPipeline().RunAsync(
            new MatchRequest { Stages = MatchStages.Cluster, Items = items, Candidates = candidates },
            TestContext.CancellationToken);

        Assert.AreEqual(-1, response.Items.Single().Cluster);
        Assert.AreEqual(4, candidates[0].Cluster);
    }

    [TestMethod]
    public async Task Run_MissingAdapterOrInput_FailsNamingIt()
    {
        var noImage = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await new MatchPipeline(new FakeDetector()).RunAsync(new MatchRequest { Kinds = MatchKinds.Faces }, TestContext.CancellationToken));
        var noDetector = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await new MatchPipeline().RunAsync(new MatchRequest { Image = Image, Kinds = MatchKinds.Pets }, TestContext.CancellationToken));
        var noEmbedding = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await new MatchPipeline().RunAsync(
                new MatchRequest { Stages = MatchStages.Cluster, Items = [new MatchedItem { Detection = Face(1) }] },
                TestContext.CancellationToken));
        var noSpecies = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await new MatchPipeline(null, null, new FakeDetector(new DetectedItem(MatchKind.Pet, default, 0.5))).RunAsync(
                new MatchRequest { Image = Image, Kinds = MatchKinds.Pets, Stages = MatchStages.Detect },
                TestContext.CancellationToken));
        TestContext.WriteLine($"{noImage.Message} | {noDetector.Message} | {noEmbedding.Message} | {noSpecies.Message}");

        StringAssert.Contains(noImage.Message, "image");
        StringAssert.Contains(noDetector.Message, "IPetDetector");
        StringAssert.Contains(noEmbedding.Message, "embedding");
        StringAssert.Contains(noSpecies.Message, "species");
    }
}
