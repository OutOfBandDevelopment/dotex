using System.Buffers;
using System.Collections.Generic;

namespace OoBDev.DevTerm.Core.TestPlugin;

/// <summary>Renders each read as <c>sample: N bytes</c>.</summary>
public sealed class ByteCountPresenter : IPresenter
{
    /// <inheritdoc />
    public string Name => "sample";

    /// <inheritdoc />
    public IReadOnlyList<string> Render(ReadOnlySequence<byte> data) => [$"sample: {data.Length} bytes"];
}
