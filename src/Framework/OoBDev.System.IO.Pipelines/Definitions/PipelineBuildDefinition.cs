using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Pipelines.Definitions;

internal sealed class PipelineBuildDefinition : IPipelineBuildDefinition
{
    internal PipelineBuildDefinition(Pipe pipe) => Pipe = pipe;

    internal Pipe Pipe { get; }
    internal PipelineErrorHandler? OnError { get; set; }
    internal Task? PipeWriter { get; set; }
    internal PipelineErrorHandler? OnWriterError { get; set; }
    internal Task? PipeReader { get; set; }
    internal PipelineErrorHandler? OnReaderError { get; set; }
    internal CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
}