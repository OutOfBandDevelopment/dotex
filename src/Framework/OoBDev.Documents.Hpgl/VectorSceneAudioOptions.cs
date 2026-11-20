using System;

namespace OoBDev.Documents.Hpgl;

/// <summary>Settings for <see cref="VectorSceneAudioWriter"/>.</summary>
public sealed class VectorSceneAudioOptions
{
    /// <summary>Samples per second; 48000 is the baseline, faster converters allow denser scenes.</summary>
    public int SampleRate { get; set; } = 48000;

    /// <summary>How long the signal lasts; the scene repeats to fill it.</summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Redraws of the scene per second; fewer frames mean more points per frame.</summary>
    public int FramesPerSecond { get; set; } = 25;

    /// <summary>Samples spent moving between two polylines; a short jump is nearly invisible without a blanking track.</summary>
    public int JumpSamples { get; set; } = 4;

    /// <summary>Checks the values and throws when one cannot produce a signal.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A value is zero or negative.</exception>
    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(SampleRate, 0, nameof(SampleRate));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Duration, TimeSpan.Zero, nameof(Duration));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(FramesPerSecond, 0, nameof(FramesPerSecond));
        ArgumentOutOfRangeException.ThrowIfNegative(JumpSamples, nameof(JumpSamples));
    }
}
