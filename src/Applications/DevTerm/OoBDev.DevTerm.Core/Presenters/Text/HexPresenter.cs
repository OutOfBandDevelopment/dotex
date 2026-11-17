using System;
using System.Buffers;
using System.Collections.Generic;

namespace OoBDev.DevTerm.Presenters.Text;

public sealed class HexPresenter : IPresenter, IPresenterInput, ISnapshotPresenter
{
    public string Name => "hex";

    /// <inheritdoc />
    public object? Snapshot() => null;

    /// <inheritdoc />
    public void Restore(object? snapshot)
    {
    }

    public IReadOnlyList<string> Render(ReadOnlySequence<byte> data) => [Convert.ToHexString(data.ToContiguousSpan())];

    public byte[] Parse(string input)
    {
        var cleaned = input.Replace(" ", string.Empty).Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase);
        return Convert.FromHexString(cleaned);
    }
}
