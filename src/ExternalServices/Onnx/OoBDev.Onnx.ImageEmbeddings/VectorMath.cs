using System;
using System.Numerics.Tensors;

namespace OoBDev.Onnx.ImageEmbeddings;

internal static class VectorMath
{
    /// <summary>Scales the vector to unit length; a zero vector is left as is.</summary>
    public static void Normalize(Span<float> vector)
    {
        var norm = TensorPrimitives.Norm(vector);
        if (norm > 0f) TensorPrimitives.Divide(vector, norm, vector);
    }

    /// <summary>Softmax over the logits, in place.</summary>
    public static void Softmax(Span<float> logits)
    {
        var max = TensorPrimitives.Max(logits);
        TensorPrimitives.Subtract(logits, max, logits);
        TensorPrimitives.Exp(logits, logits);
        var sum = TensorPrimitives.Sum(logits);
        TensorPrimitives.Divide(logits, sum, logits);
    }
}
