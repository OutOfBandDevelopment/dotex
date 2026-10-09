using System;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>
/// Resize, crop, scale and normalise, following a Hugging Face <c>preprocessor_config.json</c>.
/// Output is channel first (R plane, G plane, B plane) float32.
/// </summary>
internal sealed class ImagePreprocessor(OnnxImageEmbeddingOptions options)
{
    private readonly int _size = options.ImageSize;
    private readonly int? _shortestEdge = options.ResizeShortestEdge;
    private readonly bool _crop = options.CenterCrop;
    private readonly ImageResample _resample = options.Resample;
    private readonly float[] _mean = [.. options.Mean];
    private readonly float[] _std = [.. options.Std];

    /// <summary>Number of floats written for one image.</summary>
    public int Length => 3 * _size * _size;

    /// <summary>Preprocesses one decoded image into <paramref name="destination"/>.</summary>
    public void Process(DecodedImage image, Span<float> destination)
    {
        var (width, height) = TargetSize(image.Width, image.Height);
        var pixels = ImageResizer.Resize(image.Rgb, image.Width, image.Height, width, height, _resample);

        var left = 0;
        var top = 0;
        if (_crop)
        {
            left = (width - _size) / 2;
            top = (height - _size) / 2;
        }

        var plane = _size * _size;
        const float scale = 1f / 255f;
        for (var y = 0; y < _size; y++)
        {
            for (var x = 0; x < _size; x++)
            {
                var source = (((top + y) * width) + left + x) * 3;
                var index = (y * _size) + x;
                for (var c = 0; c < 3; c++) destination[(c * plane) + index] = ((pixels[source + c] * scale) - _mean[c]) / _std[c];
            }
        }
    }

    private (int Width, int Height) TargetSize(int width, int height)
    {
        if (_shortestEdge is not { } edge) return (_size, _size);
        // Same rule as the Hugging Face resize: the shorter edge becomes `edge`, the longer is truncated.
        if (width <= height) return (edge, Math.Max(1, (int)((long)edge * height / width)));
        return (Math.Max(1, (int)((long)edge * width / height)), edge);
    }
}
