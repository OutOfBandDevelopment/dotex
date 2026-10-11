using System;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>Resampling filter, numbered like Pillow's <c>Image.Resampling</c>.</summary>
public enum ImageResample
{
    /// <summary>Triangle filter (Pillow <c>BILINEAR</c>, value 2).</summary>
    Bilinear = 2,

    /// <summary>Cubic filter with a = -0.5 (Pillow <c>BICUBIC</c>, value 3).</summary>
    Bicubic = 3,
}
