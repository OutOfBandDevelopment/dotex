using System.Buffers;
using System.Collections.Generic;

namespace OoBDev.DevTerm.Presenters.Text;

public sealed class BinaryPresenter : IPresenter, IPresenterInput, ISnapshotPresenter
{
    public string Name => "binary";

    /// <inheritdoc />
    public object? Snapshot() => null;

    /// <inheritdoc />
    public void Restore(object? snapshot)
    {
    }

    public IReadOnlyList<string> Render(ReadOnlySequence<byte> data) => [NumericBasePresenter.Render(data, toBase: 2, width: 8)];

    public byte[] Parse(string input) => NumericBasePresenter.Parse(input, fromBase: 2);
}
