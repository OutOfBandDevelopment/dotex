using System;

namespace OoBDev.Documents.Hpgl;

/// <summary>Turns the bytes of a vector language into a <see cref="VectorScene"/>; never throws on malformed input.</summary>
public interface IVectorSceneParser
{
    /// <summary>Parses <paramref name="data"/>.</summary>
    VectorScene Parse(ReadOnlySpan<byte> data);
}
