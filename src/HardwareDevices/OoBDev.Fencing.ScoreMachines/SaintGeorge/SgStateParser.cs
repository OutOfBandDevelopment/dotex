using OoBDev.System;
using System;
using System.Linq;
using static OoBDev.System.IO.Bytes;

namespace OoBDev.Fencing.ScoreMachines.SaintGeorge;

/// <summary>
/// Parses Saint George frames (<c>SOH</c> ... <c>EOT</c>). Each frame carries one kind of fact (score, clock, lamps, cards or
/// priority), so the parser keeps the last state and returns it with the new fact applied. A frame it does not recognise returns the
/// last state unchanged.
/// </summary>
public sealed class SgStateParser : IParseScoreMachineState
{
    private IScoreMachineState _last = ScoreMachineState.Empty;

    /// <inheritdoc/>
    public IScoreMachineState? Parse(ReadOnlySpan<byte> frame)
    {
        if (frame.Length == 0 || frame[0] != Soh || frame[^1] != Eotr) return _last;

        if (frame.StartsWith(Soh, Dc3, S, T, Sotx)) return ParseScoreAndCards(frame);
        if (frame.StartsWith(Soh, Dc3, L, R) && frame.Length > 10 && frame[5] == G && frame[7] == W && frame[9] == w) return ParseLights(frame);
        if (frame.StartsWith(Soh, Dc3, R, _, F, _S, Sotx) && frame.Length > 12) return ParseLongClock(frame);
        if (frame.StartsWith(Soh, Dc2, P)) return ParsePriority(frame);
        if (frame.StartsWith(Soh, Dc3, S)) return ParseShortScore(frame);
        if (frame.StartsWith(Soh, Dc3, T)) return ParseShortClock(frame);
        return _last;
    }

    // <SOH><DC3>ST<STX>011:005<STX>..<STX>..: right score, ':', left score (three digits each), then the green and red card digits.
    private IScoreMachineState ParseScoreAndCards(ReadOnlySpan<byte> frame)
    {
        var subFrames = new Memory<byte>(frame.ToArray()).Split(Sotx).ToArray();
        var scores = subFrames.ElementAt(1).Split(_C).ToArray();

        static byte GetScore(Memory<byte> s) => (byte)((s.Span[0] - _0) * 100 + (s.Span[1] - _0) * 10 + (s.Span[2] - _0));

        var greenCards = (Cards)(subFrames.ElementAtOrDefault(2).Span[1] - _0);
        var redCards = (Cards)(subFrames.ElementAtOrDefault(3).Span[1] - _0);

        var red = _last.Left with { Score = GetScore(scores[1]), Cards = redCards };
        var green = _last.Right with { Score = GetScore(scores[0]), Cards = greenCards };
        return _last = new ScoreMachineState(red, green, _last.Clock, _last.Match);
    }

    // <SOH><DC3>LR0G1W0w0: touch left, touch right, off-target left, off-target right (1 = lit).
    private IScoreMachineState ParseLights(ReadOnlySpan<byte> frame)
    {
        static Lights Lamps(byte touch, byte offTarget) =>
            (touch == _1 ? Lights.Touch : Lights.None) | (offTarget == _1 ? Lights.White : Lights.None);

        var red = _last.Left with { Lights = Lamps(frame[4], frame[8]) };
        var green = _last.Right with { Lights = Lamps(frame[6], frame[10]) };
        return _last = new ScoreMachineState(red, green, _last.Clock, _last.Match);
    }

    // <SOH><DC3>R_F$<STX>....__:MM:SS.___
    private IScoreMachineState ParseLongClock(ReadOnlySpan<byte> frame)
    {
        var chunk = new Memory<byte>(frame.ToArray()).Split(Sotx).ElementAt(1).Span;
        var minutes = (chunk[8] - _0) * 10 + chunk[9] - _0;
        var seconds = (chunk[11] - _0) * 10 + chunk[12] - _0;
        return _last = new ScoreMachineState(_last.Left, _last.Right, new TimeSpan(0, minutes, seconds), _last.Match);
    }

    // <SOH><DC2>P?AA8G. The source computes both priorities from the same byte (the 'P' itself), which is never set, so priority is
    // always off. Kept as the source has it until a real capture settles which byte carries it.
    private IScoreMachineState ParsePriority(ReadOnlySpan<byte> frame)
    {
        var priority = (frame[2] & Eotx) == 0x01;
        return _last = new ScoreMachineState(_last.Left with { Priority = priority }, _last.Right with { Priority = priority }, _last.Clock, _last.Match);
    }

    // <SOH><DC3>S01:06: right score, ':', left score (two digits each).
    private IScoreMachineState ParseShortScore(ReadOnlySpan<byte> frame)
    {
        var scores = new Memory<byte>(frame.ToArray()).Split(S).ElementAt(1).Split(_C).ToArray();

        static byte GetScore(Memory<byte> s) => (byte)((s.Span[0] - _0) * 10 + (s.Span[1] - _0));

        return _last = new ScoreMachineState(
            _last.Left with { Score = GetScore(scores[1]) },
            _last.Right with { Score = GetScore(scores[0]) },
            _last.Clock,
            _last.Match);
    }

    // <SOH><DC3>T02:16
    private IScoreMachineState ParseShortClock(ReadOnlySpan<byte> frame)
    {
        var minutes = (frame[3] - _0) * 10 + frame[4] - _0;
        var seconds = (frame[6] - _0) * 10 + frame[7] - _0;
        return _last = new ScoreMachineState(_last.Left, _last.Right, new TimeSpan(0, minutes, seconds), _last.Match);
    }
}
