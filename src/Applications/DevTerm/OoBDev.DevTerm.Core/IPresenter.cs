using System.Buffers;
using System.Collections.Generic;

namespace OoBDev.DevTerm;

/// <summary>Turns received bytes into viewable text.</summary>
public interface IPresenter
{
    /// <summary>Gets the stable, unique name used to select this presenter, for example <c>hex</c>.</summary>
    string Name { get; }

    /// <summary>
    /// Renders a chunk. A presenter may buffer a partial item (a line, a frame) and return nothing until it is complete,
    /// or return several items when more than one boundary arrived in the chunk.
    /// </summary>
    IReadOnlyList<string> Render(ReadOnlySequence<byte> data);
}
