using Microsoft.Extensions.Options;
using OoBDev.Documents.Models;
using OoBDev.Documents.Scanning;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ClamAv;

/// <summary>
/// Scans content with a ClamAV daemon over its <c>INSTREAM</c> TCP command: the content is sent in length-prefixed
/// chunks and clamd answers <c>stream: OK</c>, <c>stream: &lt;signature&gt; FOUND</c> or an <c>ERROR</c>. A scan that
/// cannot finish (connection, timeout, clamd error such as a size limit) throws, so the caller treats the content as
/// unscanned and refuses it (<c>ScanningBlobContainer</c> does).
/// </summary>
/// <param name="options">Host, port and limits.</param>
public sealed class ClamAvContentScanner(IOptions<ClamAvOptions> options) : IContentScanner
{
    /// <inheritdoc />
    public async Task<ContentScanResult> ScanAsync(ContentReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        var settings = options.Value;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(settings.TimeoutSeconds));
        var token = timeout.Token;

        using var client = new TcpClient();
        await client.ConnectAsync(settings.Host, settings.Port, token).ConfigureAwait(false);
        var stream = client.GetStream();
        await using var streamDisposer = stream.ConfigureAwait(false);

        await stream.WriteAsync(Encoding.ASCII.GetBytes("zINSTREAM\0"), token).ConfigureAwait(false);

        var buffer = ArrayPool<byte>.Shared.Rent(settings.ChunkSize + 4);
        try
        {
            int read;
            while ((read = await reference.Content.ReadAsync(buffer.AsMemory(4, settings.ChunkSize), token).ConfigureAwait(false)) > 0)
            {
                BinaryPrimitives.WriteInt32BigEndian(buffer, read);
                await stream.WriteAsync(buffer.AsMemory(0, read + 4), token).ConfigureAwait(false);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        await stream.WriteAsync(new byte[4], token).ConfigureAwait(false);
        await stream.FlushAsync(token).ConfigureAwait(false);

        return Parse(await ReadReplyAsync(stream, token).ConfigureAwait(false));
    }

    private static async Task<string> ReadReplyAsync(Stream stream, CancellationToken token)
    {
        using var reply = new MemoryStream();
        var one = new byte[1];
        while (await stream.ReadAsync(one, token).ConfigureAwait(false) == 1 && one[0] != 0 && one[0] != (byte)'\n')
        {
            reply.WriteByte(one[0]);
        }

        return Encoding.UTF8.GetString(reply.ToArray()).Trim();
    }

    internal static ContentScanResult Parse(string reply)
    {
        if (reply.EndsWith("OK", StringComparison.Ordinal))
        {
            return ContentScanResult.Clean;
        }

        if (reply.EndsWith("FOUND", StringComparison.Ordinal))
        {
            var signature = reply["stream:".Length..^"FOUND".Length].Trim();
            return ContentScanResult.Infected(signature.Length == 0 ? "malware found" : signature);
        }

        throw new InvalidOperationException($"ClamAV could not scan the content: {(reply.Length == 0 ? "no reply" : reply)}");
    }
}
