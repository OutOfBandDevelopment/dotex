using OoBDev.System.IO.Pipelines.Definitions;
using OoBDev.System.IO.Pipelines.Factories;
using OoBDev.System.IO.Segmenters;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Pipelines;

/// <summary>
/// Provides extension methods for building and configuring pipeline definitions.
/// </summary>
public static class PipelineBuilder
{
    /// <summary>
    /// Creates a pipeline that follows a stream, reading data from it.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="minimumBufferSize">The minimum buffer size for reading (default is 4096 bytes).</param>
    /// <returns>A pipeline build definition configured to read from the stream.</returns>
    public static IPipelineBuildDefinition Follow(this Stream stream, int minimumBufferSize = 4096) =>
        new Pipe().FollowStream(stream, minimumBufferSize);
    internal static IPipelineBuildDefinition FollowStream(this Pipe pipe, Stream stream, int minimumBufferSize = 4096) =>
        new PipelineBuildDefinition(pipe).FollowStream(stream, minimumBufferSize);

    internal static IPipelineBuildDefinition FollowStream(this IPipelineBuildDefinition pipeline, Stream stream, int minimumBufferSize = 4096)
    {
        if (pipeline is not PipelineBuildDefinition def)
        {
            throw new NotSupportedException($"{pipeline.GetType()} is not supported");
        }
        else if (def.PipeWriter != null)
        {
            throw new NotSupportedException("this pipeline already has a writer");
        }
        def.PipeWriter = new StreamPipelineFactory().CreateWriter(def, stream, minimumBufferSize);
        return def;
    }

    /// <summary>
    /// Configures the pipeline with a segmenter for processing data.
    /// </summary>
    /// <param name="pipeline">The pipeline build definition to configure.</param>
    /// <param name="segmenter">The segmenter to use for processing data.</param>
    /// <returns>The configured pipeline build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown if the pipeline type is not supported or already has a reader.</exception>
    public static IPipelineBuildDefinition With(this IPipelineBuildDefinition pipeline, ISegmenter segmenter)
    {
        if (pipeline is not PipelineBuildDefinition def)
        {
            throw new NotSupportedException($"{pipeline.GetType()} is not supported");
        }
        else if (def.PipeReader != null)
        {
            throw new NotSupportedException("this pipeline already has a reader");
        }
        def.PipeReader = new SegmentPipeFactory().CreateReader(def, segmenter);
        return def;
    }

    /// <summary>
    /// Configures the pipeline to write data to a stream.
    /// </summary>
    /// <param name="pipeline">The pipeline build definition to configure.</param>
    /// <param name="stream">The stream to write data to.</param>
    /// <returns>The configured pipeline build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown if the pipeline type is not supported or already has a reader.</exception>
    public static IPipelineBuildDefinition With(this IPipelineBuildDefinition pipeline, Stream stream)
    {
        if (pipeline is not PipelineBuildDefinition def)
        {
            throw new NotSupportedException($"{pipeline.GetType()} is not supported");
        }
        else if (def.PipeReader != null)
        {
            throw new NotSupportedException("this pipeline already has a reader");
        }
        def.PipeReader = new StreamPipelineFactory().CreateReader(pipeline: def, stream);
        return def;
    }

    /// <summary>
    /// Registers an error handler for general pipeline errors.
    /// </summary>
    /// <param name="pipeline">The pipeline build definition to configure.</param>
    /// <param name="onPipelineError">The error handler to invoke when pipeline errors occur.</param>
    /// <returns>The configured pipeline build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown if the pipeline type is not supported or already has a reader.</exception>
    public static IPipelineBuildDefinition OnError(this IPipelineBuildDefinition pipeline, OnException onPipelineError)
    {
        if (pipeline is not PipelineBuildDefinition def)
        {
            throw new NotSupportedException($"{pipeline.GetType()} is not supported");
        }
        else if (def.PipeReader != null)
        {
            throw new NotSupportedException("this pipeline already has a reader");
        }
        def.OnError = onPipelineError;
        return def;
    }

    /// <summary>
    /// Registers an error handler specifically for pipeline reader errors.
    /// </summary>
    /// <param name="pipeline">The pipeline build definition to configure.</param>
    /// <param name="onPipelineError">The error handler to invoke when reader errors occur.</param>
    /// <returns>The configured pipeline build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown if the pipeline type is not supported or already has a reader.</exception>
    public static IPipelineBuildDefinition OnReaderError(this IPipelineBuildDefinition pipeline, OnException onPipelineError)
    {
        if (pipeline is not PipelineBuildDefinition def)
        {
            throw new NotSupportedException($"{pipeline.GetType()} is not supported");
        }
        else if (def.PipeReader != null)
        {
            throw new NotSupportedException("this pipeline already has a reader");
        }
        def.OnReaderError = onPipelineError;
        return def;
    }

    /// <summary>
    /// Registers an error handler specifically for pipeline writer errors.
    /// </summary>
    /// <param name="pipeline">The pipeline build definition to configure.</param>
    /// <param name="onPipelineError">The error handler to invoke when writer errors occur.</param>
    /// <returns>The configured pipeline build definition.</returns>
    /// <exception cref="NotSupportedException">Thrown if the pipeline type is not supported or already has a reader.</exception>
    public static IPipelineBuildDefinition OnWriterError(this IPipelineBuildDefinition pipeline, OnException onPipelineError)
    {
        if (pipeline is not PipelineBuildDefinition def)
        {
            throw new NotSupportedException($"{pipeline.GetType()} is not supported");
        }
        else if (def.PipeReader != null)
        {
            throw new NotSupportedException("this pipeline already has a reader");
        }
        def.OnWriterError = onPipelineError;
        return def;
    }

    /// <summary>
    /// Runs the configured pipeline asynchronously.
    /// </summary>
    /// <param name="pipeline">The pipeline build definition to run.</param>
    /// <param name="cancellationToken">Optional cancellation token to stop the pipeline.</param>
    /// <returns>A task representing the asynchronous pipeline execution.</returns>
    /// <exception cref="NotSupportedException">Thrown if the pipeline type is not supported, or is missing a reader or writer.</exception>
    public static Task RunAsync(this IPipelineBuildDefinition pipeline, CancellationToken cancellationToken = default)
    {
        if (pipeline is not PipelineBuildDefinition def)
        {
            throw new NotSupportedException($"{pipeline.GetType()} is not supported");
        }
        else if (def.PipeWriter == null)
        {
            throw new NotSupportedException("this pipeline is not configured with a writer");
        }
        else if (def.PipeReader == null)
        {
            throw new NotSupportedException("this pipeline is not configured with a reader");
        }

        cancellationToken.Register(def.CancellationTokenSource.Cancel);

        return Task.WhenAll(
            Task.Run(async () => await def.PipeWriter, cancellationToken),
            Task.Run(async () => await def.PipeReader, cancellationToken)
            );
    }
}