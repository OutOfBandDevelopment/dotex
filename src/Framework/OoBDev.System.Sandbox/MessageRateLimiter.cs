using System;

namespace OoBDev.System.Sandbox;

/// <summary>
/// Counts the frames a plugin sends per one second window (protocol-and-manifest.md, Table 8): frames over the limit are dropped and counted,
/// and a plugin that stays over the limit for several windows in a row is declared abusive.
/// </summary>
public sealed class MessageRateLimiter
{
    private readonly int _perSecond;
    private readonly int _sustainedWindows;
    private readonly TimeProvider _time;
    private long _windowStart;
    private int _inWindow;
    private int _overWindows;
    private bool _windowWasOver;

    /// <summary>Creates a limiter that allows <paramref name="perSecond"/> frames a second and flags <paramref name="sustainedWindows"/> consecutive windows over it.</summary>
    public MessageRateLimiter(int perSecond, int sustainedWindows = 3, TimeProvider? time = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(perSecond, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(sustainedWindows, 1);
        _perSecond = perSecond;
        _sustainedWindows = sustainedWindows;
        _time = time ?? TimeProvider.System;
        _windowStart = _time.GetTimestamp();
    }

    /// <summary>The number of frames dropped so far.</summary>
    public long Dropped { get; private set; }

    /// <summary>True once the plugin was over the limit for the configured number of consecutive windows.</summary>
    public bool Sustained => _overWindows >= _sustainedWindows;

    /// <summary>Records one received frame; false means drop it.</summary>
    public bool Allow()
    {
        var now = _time.GetTimestamp();
        if (_time.GetElapsedTime(_windowStart, now) >= TimeSpan.FromSeconds(1))
        {
            var idle = _time.GetElapsedTime(_windowStart, now) >= TimeSpan.FromSeconds(2);
            _overWindows = _windowWasOver && !idle ? _overWindows + 1 : 0;
            _windowStart = now;
            _inWindow = 0;
            _windowWasOver = false;
        }

        if (++_inWindow <= _perSecond)
        {
            return true;
        }

        _windowWasOver = true;
        Dropped++;
        return false;
    }
}
