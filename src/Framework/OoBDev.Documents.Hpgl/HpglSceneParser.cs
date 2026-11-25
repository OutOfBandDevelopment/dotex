using System;

namespace OoBDev.Documents.Hpgl;

/// <summary>The HP-GL <see cref="IVectorSceneParser"/>.</summary>
public sealed class HpglSceneParser : IVectorSceneParser
{
    /// <inheritdoc />
    public VectorScene Parse(ReadOnlySpan<byte> data) => HpglToSvgConverter.ParseScene(data);
}
