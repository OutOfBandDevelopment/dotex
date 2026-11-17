using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.DevTerm.Presenters.Text;

/// <summary>
/// Decodes UTF-8 statefully across chunks via a per-instance <see cref="Decoder"/> - a multi-byte
/// character can arrive split across two transport reads (serial routinely delivers one byte per
/// read, and TCP can split anywhere). Safe to
/// keep as instance state since presenters are resolved fresh per session (see CLAUDE.md).
/// </summary>
public sealed class Utf8Presenter : IPresenter, IPresenterInput, ISnapshotPresenter
{
    private Decoder _decoder = Encoding.UTF8.GetDecoder();

    // The Decoder keeps its half-read character private, so the presenter tracks the same bytes itself to be able to rebuild it.
    private byte[] _tail = [];

    public string Name => "utf8";

    public IReadOnlyList<string> Render(ReadOnlySequence<byte> data)
    {
        var bytes = data.ToContiguousSpan();
        if (bytes.Length == 0)
        {
            return [];
        }

        _tail = IncompleteTail([.. _tail, .. bytes]);
        var chars = new char[_decoder.GetCharCount(bytes, flush: false)];
        var charCount = _decoder.GetChars(bytes, chars, flush: false);
        return charCount == 0 ? [] : [new string(chars, 0, charCount)];
    }

    public byte[] Parse(string input) => Encoding.UTF8.GetBytes(input);

    /// <inheritdoc />
    public object? Snapshot() => _tail;

    /// <inheritdoc />
    public void Restore(object? snapshot)
    {
        var tail = snapshot as byte[] ?? throw new ArgumentException("Not a Utf8Presenter snapshot.", nameof(snapshot));
        _decoder = Encoding.UTF8.GetDecoder();
        _tail = tail;
        if (tail.Length > 0)
        {
            _decoder.GetChars(tail, new char[tail.Length], flush: false);
        }
    }

    private static byte[] IncompleteTail(byte[] bytes)
    {
        for (var back = 1; back <= Math.Min(3, bytes.Length); back++)
        {
            var b = bytes[^back];
            if ((b & 0xC0) == 0x80)
            {
                continue;
            }

            var length = b >= 0xF0 && b <= 0xF7 ? 4 : b >= 0xE0 ? 3 : b >= 0xC2 ? 2 : 1;
            return b >= 0xC2 && b <= 0xF7 && back < length ? bytes[^back..] : [];
        }

        return [];
    }
}
