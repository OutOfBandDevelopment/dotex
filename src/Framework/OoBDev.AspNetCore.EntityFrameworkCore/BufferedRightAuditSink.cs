using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Channels;

namespace OoBDev.AspNetCore.EntityFrameworkCore;

/// <summary>
/// A bounded buffer between the request path and the database. <see cref="Record"/> never blocks or throws; when the buffer
/// is full the entry is dropped and counted in <see cref="Dropped"/>, because an audit write must not stop a request.
/// </summary>
/// <param name="options">The buffer size.</param>
public sealed class BufferedRightAuditSink(IOptions<RightAuditOptions> options) : IRightAuditSink
{
    private long dropped;

    /// <summary>Gets the entries waiting to be written.</summary>
    public Channel<RightAuditEntry> Channel { get; } = global::System.Threading.Channels.Channel.CreateBounded<RightAuditEntry>(
        new BoundedChannelOptions(options.Value.QueueCapacity) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true });

    /// <summary>Gets how many entries were dropped because the buffer was full.</summary>
    public long Dropped => Interlocked.Read(ref dropped);

    /// <inheritdoc />
    public void Record(RightAuditEntry entry)
    {
        if (!Channel.Writer.TryWrite(entry))
        {
            Interlocked.Increment(ref dropped);
        }
    }
}
