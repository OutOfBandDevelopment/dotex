using System.Buffers;
using System.Collections.Generic;

namespace OoBDev.DevTerm.Presenters.Text;

public sealed class OctalPresenter : IPresenter, IPresenterInput, ISnapshotPresenter
{
    public string Name => "octal";

    /// <inheritdoc />
    public object? Snapshot() => null;

    /// <inheritdoc />
    public void Restore(object? snapshot)
    {
    }

    public IReadOnlyList<string> Render(ReadOnlySequence<byte> data) => [NumericBasePresenter.Render(data, toBase: 8, width: 3)];

    public byte[] Parse(string input) => NumericBasePresenter.Parse(input, fromBase: 8);
}
