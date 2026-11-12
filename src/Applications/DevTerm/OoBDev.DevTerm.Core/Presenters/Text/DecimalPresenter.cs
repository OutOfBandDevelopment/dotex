using System.Buffers;
using System.Collections.Generic;

namespace OoBDev.DevTerm.Presenters.Text;

public sealed class DecimalPresenter : IPresenter, IPresenterInput, ISnapshotPresenter
{
    public string Name => "decimal";

    /// <inheritdoc />
    public object? Snapshot() => null;

    /// <inheritdoc />
    public void Restore(object? snapshot)
    {
    }

    public IReadOnlyList<string> Render(ReadOnlySequence<byte> data) => [NumericBasePresenter.Render(data, toBase: 10, width: 1)];

    public byte[] Parse(string input) => NumericBasePresenter.Parse(input, fromBase: 10);
}
