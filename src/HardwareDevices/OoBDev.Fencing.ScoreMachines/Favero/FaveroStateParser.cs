using OoBDev.System;
using System;

namespace OoBDev.Fencing.ScoreMachines.Favero;

/// <summary>
/// Parses the 10-byte Favero frame: <c>FF</c>, right score, left score, seconds, minutes, lamps, match and priority, a byte reserved
/// by the maker, cards and a checksum. The checksum is not verified: the source never found a formula that matched real frames.
/// </summary>
public sealed class FaveroStateParser : IParseScoreMachineState
{
    /// <summary>The frame length.</summary>
    public const int FrameLength = 10;

    /// <summary>The first byte of every frame.</summary>
    public const byte Start = 0xff;

    /// <inheritdoc/>
    public IScoreMachineState? Parse(ReadOnlySpan<byte> frame)
    {
        if (frame.Length == 0) return null;
        if (frame[0] != Start) throw new ArgumentException("invalid prefix", nameof(frame));
        if (frame.Length < FrameLength) throw new ArgumentException("frame is shorter than 10 bytes", nameof(frame));

        var lamps = ParseLights(frame[5]);
        var priority = ParsePriority(frame[6]);
        var cards = ParseCards(frame[8]);

        return new ScoreMachineState(
            Left: new Fencer((byte)frame[2].AsBCD(), cards.left, lamps.left, priority.left),
            Right: new Fencer((byte)frame[1].AsBCD(), cards.right, lamps.right, priority.right),
            Clock: new TimeSpan(0, ((byte)(frame[4] & 0x0f)).AsBCD(), frame[3].AsBCD()),
            Match: (byte)(frame[6] & 0x3));
    }

    // D0 left white, D1 right white, D2 red (left), D3 green (right), D4 right yellow, D5 left yellow.
    private static (Lights left, Lights right) ParseLights(byte lamps) => (
        left: (Lights)((lamps & 0x5) | (lamps >> 4 & 0x2)),
        right: (Lights)(((lamps >> 1) & 0x5) | (lamps >> 3 & 0x2)));

    // D2 right priority, D3 left priority (D0 and D1 hold the match number).
    private static (bool left, bool right) ParsePriority(byte flags) => (
        left: (flags & 0x08) != 0x00,
        right: (flags & 0x04) != 0x00);

    // D0 right red, D1 left red, D2 right yellow, D3 left yellow.
    private static (Cards left, Cards right) ParseCards(byte cards) => (
        left: Card((byte)((cards >> 1) & 0x5)),
        right: Card((byte)(cards & 0x5)));

    private static Cards Card(byte bits) =>
        ((bits & 0x01) != 0 ? Cards.Red : Cards.None) | ((bits & 0x04) != 0 ? Cards.Yellow : Cards.None);
}
