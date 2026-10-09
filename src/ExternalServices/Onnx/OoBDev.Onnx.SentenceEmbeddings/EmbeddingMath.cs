using System;
using System.Numerics.Tensors;

namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// Pooling and normalisation over spans. Pure functions: no shared state.
/// </summary>
internal static class EmbeddingMath
{
    /// <summary>
    /// Pools one row of token vectors.
    /// </summary>
    /// <param name="tokens">Row-major token vectors, <c>sequenceLength * hidden</c> values.</param>
    /// <param name="mask">Attention mask for the row, one entry per token.</param>
    /// <param name="hidden">Size of one token vector.</param>
    /// <param name="pooling">Pooling mode.</param>
    /// <param name="destination">Receives <paramref name="hidden"/> values.</param>
    public static void Pool(ReadOnlySpan<float> tokens, ReadOnlySpan<long> mask, int hidden, EmbeddingPooling pooling, Span<float> destination)
    {
        destination.Clear();
        if (pooling == EmbeddingPooling.Cls)
        {
            tokens[..hidden].CopyTo(destination);
            return;
        }

        var count = 0;
        for (var t = 0; t < mask.Length; t++)
        {
            if (mask[t] == 0) continue;
            TensorPrimitives.Add(destination, tokens.Slice(t * hidden, hidden), destination);
            count++;
        }
        if (count > 1) TensorPrimitives.Divide(destination, count, destination);
    }

    /// <summary>
    /// Layer normalisation without scale or shift: zero mean and unit variance across the vector (epsilon 1e-5).
    /// </summary>
    public static void LayerNormalize(Span<float> vector)
    {
        if (vector.IsEmpty) return;
        var mean = TensorPrimitives.Sum(vector) / vector.Length;
        TensorPrimitives.Subtract(vector, mean, vector);
        var variance = TensorPrimitives.SumOfSquares(vector) / vector.Length;
        TensorPrimitives.Divide(vector, MathF.Sqrt(variance + 1e-5f), vector);
    }

    /// <summary>
    /// Scales a vector to unit length; a zero vector is left as zeros.
    /// </summary>
    public static void Normalize(Span<float> vector)
    {
        var norm = TensorPrimitives.Norm(vector);
        if (norm > 0f) TensorPrimitives.Divide(vector, norm, vector);
    }
}
