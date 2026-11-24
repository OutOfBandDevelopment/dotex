using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;

namespace OoBDev.IO.Inventory.Images;

/// <summary>Groups images whose embedding vectors are similar (cosine); the caller supplies the vectors, for example from the ONNX image embedder. Nothing is moved or deleted.</summary>
public sealed class EmbeddingMatcher
{
    /// <summary>Groups embedded images (single linkage).</summary>
    /// <param name="embeddings">Path and vector pairs; all vectors must have the same length.</param>
    /// <param name="minSimilarity">Smallest cosine similarity that counts as a match (about 0.9 for the same scene, 0.97 for near copies).</param>
    /// <returns>Groups of two or more paths, each sorted.</returns>
    public IReadOnlyList<IReadOnlyList<string>> Group(IReadOnlyList<(string Path, float[] Vector)> embeddings, float minSimilarity)
    {
        ArgumentNullException.ThrowIfNull(embeddings);
        var parent = Enumerable.Range(0, embeddings.Count).ToArray();
        int Find(int i) { while (parent[i] != i) i = parent[i] = parent[parent[i]]; return i; }

        for (var i = 0; i < embeddings.Count; i++)
        {
            for (var j = i + 1; j < embeddings.Count; j++)
            {
                if (embeddings[i].Vector.Length != embeddings[j].Vector.Length)
                {
                    throw new ArgumentException("All embedding vectors must have the same length.", nameof(embeddings));
                }

                if (TensorPrimitives.CosineSimilarity(embeddings[i].Vector, embeddings[j].Vector) >= minSimilarity) parent[Find(j)] = Find(i);
            }
        }

        return Enumerable.Range(0, embeddings.Count)
            .GroupBy(Find)
            .Where(g => g.Count() > 1)
            .Select(g => (IReadOnlyList<string>)g.Select(i => embeddings[i].Path).Order(StringComparer.OrdinalIgnoreCase).ToList())
            .OrderBy(g => g[0], StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
