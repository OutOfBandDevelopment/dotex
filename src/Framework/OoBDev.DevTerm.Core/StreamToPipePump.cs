using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DevTerm;

/// <summary>
/// Copies a readable <see cref="Stream"/> into a <see cref="PipeWriter"/>, reading straight into the pipe's pooled buffers.
/// The source's <c>ReadAsync</c> must honour cancellation; a stream that does not (the serial port base stream) has to be wrapped first.
/// </summary>
public static class StreamToPipePump
{
    private const int _minimumBufferSize = 4096;

    /// <summary>Pumps until the source ends, the token is cancelled, the reader completes or a read fails; the writer is then completed with the failure, if any.</summary>
    public static async Task RunAsync(Stream source, PipeWriter writer, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(writer);

        Exception? error = null;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var memory = writer.GetMemory(_minimumBufferSize);

                int bytesRead;
                try
                {
                    bytesRead = await source.ReadAsync(memory, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    error = ex;
                    break;
                }

                if (bytesRead == 0) break;

                writer.Advance(bytesRead);

                FlushResult flush;
                try
                {
                    flush = await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (flush.IsCompleted || flush.IsCanceled) break;
            }
        }
        finally
        {
            await writer.CompleteAsync(error).ConfigureAwait(false);
        }
    }
}
