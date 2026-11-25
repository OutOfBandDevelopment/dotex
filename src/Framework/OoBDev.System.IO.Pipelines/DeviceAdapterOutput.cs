using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Pipelines;

/// <summary>Builds the <see cref="IDeviceAdapter.Output"/> pipe for adapters whose writes need their own logic (timeouts, flow control, framing).</summary>
public static class DeviceAdapterOutput
{
    /// <summary>Creates a writer whose every flush hands the bytes to <paramref name="write"/>.</summary>
    /// <param name="write">The delegate that sends the bytes to the device.</param>
    /// <returns>The pipe writer.</returns>
    public static PipeWriter Create(Func<ReadOnlyMemory<byte>, CancellationToken, Task> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        return PipeWriter.Create(new DelegateWriteStream(write), new StreamPipeWriterOptions(leaveOpen: true));
    }
}
