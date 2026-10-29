using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.DevTerm.Tests;

/// <summary>Renders each chunk verbatim as ASCII, one output per chunk; the simplest presenter for session tests.</summary>
public sealed class RawPresenter : IPresenter
{
    public string Name => "raw";

    public IReadOnlyList<string> Render(ReadOnlySequence<byte> data) =>
        data.IsEmpty ? [] : [Encoding.ASCII.GetString(data.ToArray())];
}
