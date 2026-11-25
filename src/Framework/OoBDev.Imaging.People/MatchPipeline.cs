using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Imaging.People;

/// <summary>
/// Chains the detect, embed and cluster stages of one <see cref="MatchRequest"/>. The detectors and embedders are optional: a stage that needs one that is missing fails with a message naming it, so a host that only clusters (or only detects) needs no models.
/// </summary>
/// <param name="faceDetector">Finds faces.</param>
/// <param name="faceEmbedder">Embeds faces.</param>
/// <param name="petDetector">Finds pets.</param>
/// <param name="petEmbedder">Embeds pets.</param>
public sealed class MatchPipeline(
    IFaceDetector? faceDetector = null,
    IFaceEmbedder? faceEmbedder = null,
    IPetDetector? petDetector = null,
    IPetEmbedder? petEmbedder = null)
{
    /// <summary>Runs the stages the request asks for.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancels the run.</param>
    /// <returns>The items with the detail of the stages that ran.</returns>
    /// <exception cref="InvalidOperationException">A stage needs an image, an embedding or an adapter that is missing.</exception>
    public async ValueTask<MatchResponse> RunAsync(MatchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var items = new List<MatchedItem>(request.Items);

        if (request.Stages.HasFlag(MatchStages.Detect))
        {
            await DetectAsync(request, items, cancellationToken).ConfigureAwait(false);
        }

        if (request.Stages.HasFlag(MatchStages.Embed))
        {
            await EmbedAsync(request, items, cancellationToken).ConfigureAwait(false);
        }

        var newFaces = 0;
        IReadOnlyList<string?> newPets = [];
        if (request.Stages.HasFlag(MatchStages.Cluster))
        {
            (newFaces, newPets) = Cluster(request, items);
        }

        if (!request.IncludeEmbeddings)
        {
            for (var i = 0; i < items.Count; i++)
            {
                items[i] = items[i] with { Embedding = null };
            }
        }

        return new MatchResponse(items, request.Stages, newFaces, newPets);
    }

    private async ValueTask DetectAsync(MatchRequest request, List<MatchedItem> items, CancellationToken cancellationToken)
    {
        var image = request.Image ?? throw new InvalidOperationException("The detect stage needs an image.");
        if (request.Kinds.HasFlag(MatchKinds.Faces))
        {
            var detector = faceDetector ?? throw new InvalidOperationException("The detect stage for faces needs an IFaceDetector.");
            foreach (var found in await detector.DetectAsync(image, cancellationToken).ConfigureAwait(false))
            {
                items.Add(new MatchedItem { Detection = found with { Kind = MatchKind.Face, Species = null } });
            }
        }

        if (request.Kinds.HasFlag(MatchKinds.Pets))
        {
            var detector = petDetector ?? throw new InvalidOperationException("The detect stage for pets needs an IPetDetector.");
            foreach (var found in await detector.DetectAsync(image, cancellationToken).ConfigureAwait(false))
            {
                if (string.IsNullOrWhiteSpace(found.Species))
                {
                    throw new InvalidOperationException("The pet detector returned a detection without a species.");
                }

                if (request.Species.Count == 0 || request.Species.Contains(found.Species))
                {
                    items.Add(new MatchedItem { Detection = found with { Kind = MatchKind.Pet } });
                }
            }
        }
    }

    private async ValueTask EmbedAsync(MatchRequest request, List<MatchedItem> items, CancellationToken cancellationToken)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Embedding is not null)
            {
                continue;
            }

            var image = request.Image ?? throw new InvalidOperationException("The embed stage needs an image.");
            var detection = items[i].Detection;
            float[] vector;
            if (detection.Kind == MatchKind.Face)
            {
                var embedder = faceEmbedder ?? throw new InvalidOperationException("The embed stage for faces needs an IFaceEmbedder.");
                vector = await embedder.EmbedAsync(image, detection, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var embedder = petEmbedder ?? throw new InvalidOperationException("The embed stage for pets needs an IPetEmbedder.");
                vector = await embedder.EmbedAsync(image, detection, cancellationToken).ConfigureAwait(false);
            }

            items[i] = items[i] with { Embedding = vector };
        }
    }

    private static (int NewFaces, IReadOnlyList<string?> NewPetSpecies) Cluster(MatchRequest request, List<MatchedItem> items)
    {
        var options = request.Options ?? new PeopleClusteringOptions();
        var nextId = request.Candidates.Count == 0 ? 1 : request.Candidates.Max(c => c.Id) + 1;
        var newFaces = 0;
        IReadOnlyList<string?> newPets = [];
        foreach (var kind in new[] { MatchKind.Face, MatchKind.Pet })
        {
            var indexes = Enumerable.Range(0, items.Count).Where(i => items[i].Detection.Kind == kind).ToList();
            if (indexes.Count == 0)
            {
                continue;
            }

            var known = request.Candidates.Where(c => (c.Species is null) == (kind == MatchKind.Face)).ToList();
            var all = new List<ClusterItem>(known);
            var idOf = new Dictionary<int, long>();
            foreach (var i in indexes)
            {
                var item = items[i];
                var embedding = item.Embedding ?? throw new InvalidOperationException("The cluster stage needs an embedding; run the embed stage first.");
                var species = kind == MatchKind.Pet ? item.Detection.Species ?? throw new InvalidOperationException("A pet needs a species.") : null;
                idOf[i] = nextId;
                all.Add(new ClusterItem(nextId++, embedding, null, species));
            }

            var result = kind == MatchKind.Face
                ? CentroidClusterer.Faces(all, options)
                : CentroidClusterer.Pets(all, new HashSet<int>(request.NamedPetClusters), options);
            var byId = result.Assignments.ToDictionary(a => a.Id);
            foreach (var i in indexes)
            {
                var assignment = byId[idOf[i]];
                var embedding = items[i].Embedding!;
                var species = items[i].Detection.Species;
                var nearest = known
                    .Where(c => kind == MatchKind.Face || string.Equals(c.Species, species, StringComparison.Ordinal))
                    .Select(c => new NearCandidate(c.Id, c.Cluster, Cosine(embedding, c.Embedding)))
                    .OrderByDescending(n => n.Similarity)
                    .Take(request.NearestCount)
                    .ToList();
                items[i] = items[i] with { Cluster = assignment.Target, Similarity = assignment.Similarity, Why = assignment.Why, Nearest = nearest };
            }

            if (kind == MatchKind.Face)
            {
                newFaces = result.NewClusterSpecies.Count;
            }
            else
            {
                newPets = result.NewClusterSpecies;
            }
        }

        return (newFaces, newPets);
    }

    private static double Cosine(float[] a, float[] b)
    {
        double dot = 0, na = 0, nb = 0;
        var n = Math.Min(a.Length, b.Length);
        for (var i = 0; i < n; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }

        return na == 0 || nb == 0 ? 0 : dot / (Math.Sqrt(na) * Math.Sqrt(nb));
    }
}
