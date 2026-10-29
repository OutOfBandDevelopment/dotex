using OoBDev.System.IO.Pipelines.Definitions;
using OoBDev.System.IO.Segmenters;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Pipelines.Factories;

internal sealed class SegmentPipeFactory
{
    internal async Task CreateReader(PipelineBuildDefinition def, ISegmenter segmenter)
    {
        var context = new
        {
            pipeline = def.Pipe.Reader,
            onError = def.OnReaderError ?? def.OnError,
            cancellationToken = def.CancellationTokenSource.Token,
            owner = segmenter,
        };

        var completed = false;
        while (!context.cancellationToken.IsCancellationRequested)
        {
            var result = await context.pipeline.ReadAsync(context.cancellationToken).ConfigureAwait(false);
            try
            {
                var buffer = result.Buffer;

                buffer = await ReadSegmentsAsync(segmenter, buffer, context.cancellationToken).ConfigureAwait(false);

                // Tell the PipeReader how much of the buffer we have consumed
                context.pipeline.AdvanceTo(buffer.Start, buffer.End);
            }
            catch (Exception ex)
            {
                var errorHandling = await context.onError.Handle(context.owner, ex).ConfigureAwait(false);
                switch (errorHandling)
                {
                    case ErrorHandling.Ignore:
                        //Note: do nothing
                        break;

                    case ErrorHandling.Stop:
                        completed = true;
                        break;

                    default:
                    case ErrorHandling.Throw:
                        context.pipeline.Complete(ex);
                        return;
                }
            }

            // Stop reading if there's no more data coming
            if (result.IsCompleted || completed)
            {
                break;
            }
        }

        //Mark the PipeReader as complete
        context.pipeline.Complete();
    }

    private static async Task<ReadOnlySequence<byte>> ReadSegmentsAsync(ISegmenter segmenter, ReadOnlySequence<byte> buffer, global::System.Threading.CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var read = await segmenter.TryReadAsync(buffer).ConfigureAwait(false);
            buffer = read.RemainingData;
            if (read.Status == SegmentationStatus.Incomplete)
            {
                break;
            }
            else if (read.Status == SegmentationStatus.Invalid)
            {
                throw new InvalidSegmentationException();
            }
        }

        return buffer;
    }
}
