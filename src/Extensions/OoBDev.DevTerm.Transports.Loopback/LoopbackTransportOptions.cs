namespace OoBDev.DevTerm.Transports.Loopback;

/// <summary>Configuration of <see cref="LoopbackTransport"/>.</summary>
public sealed class LoopbackTransportOptions
{
    /// <summary>
    /// Milliseconds between the lines of a streaming scripted response (<c>Samples: N</c>, <c>Send Events: N</c>).
    /// The default 0 delivers every line back to back; single-line responses are never delayed.
    /// </summary>
    public int SampleIntervalMs { get; set; }
}
