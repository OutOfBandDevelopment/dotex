using NAudio.Wave;

namespace OoBDev.HpglPlayer.Cli;

/// <summary>Repeats a finished wave stream without a gap: when the source ends it rewinds and keeps filling the buffer.</summary>
public sealed class LoopingWaveProvider(WaveStream source) : IWaveProvider
{
    /// <inheritdoc />
    public WaveFormat WaveFormat => source.WaveFormat;

    /// <inheritdoc />
    public int Read(byte[] buffer, int offset, int count)
    {
        var total = 0;
        while (total < count)
        {
            var read = source.Read(buffer, offset + total, count - total);
            if (read == 0)
            {
                if (source.Position == 0)
                {
                    break;
                }

                source.Position = 0;
            }

            total += read;
        }

        return total;
    }
}
