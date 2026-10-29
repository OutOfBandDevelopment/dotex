namespace OoBDev.ZWave;

/// <summary>
/// Reads command class reports out of the <see cref="ZWaveFunctions.ApplicationCommandHandler"/> frames a node causes the stick to send.
/// </summary>
public static class ZWaveReports
{
    private const byte BinarySwitchClass = 0x25;
    private const byte BinarySwitchReport = 0x03;

    /// <summary>
    /// Reads a Binary Switch Report: <c>04 status node len 25 03 value</c>.
    /// </summary>
    /// <param name="frame">A decoded frame.</param>
    /// <param name="node">The node that reported.</param>
    /// <param name="on">Whether the node reports on (any non-zero value).</param>
    /// <returns><see langword="true"/> when <paramref name="frame"/> is a Binary Switch Report.</returns>
    public static bool TryReadBinarySwitch(ZWaveFrame frame, out byte node, out bool on)
    {
        node = 0;
        on = false;
        if (frame.Kind != ZWaveFrameKind.Data || frame.Type != ZWaveFrame.Request || frame.Function != ZWaveFunctions.ApplicationCommandHandler)
            return false;

        var payload = frame.Payload;
        if (payload.Length < 6 || payload[3] != BinarySwitchClass || payload[4] != BinarySwitchReport)
            return false;

        node = payload[1];
        on = payload[5] != 0;
        return true;
    }
}
