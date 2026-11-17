using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Media.Metadata.Raw;

/// <summary>
/// The always-available developer: it writes the largest JPEG preview the camera embedded in the RAW file, with no native code. This is the camera's own rendering, not a development,
/// so any option beyond 8-bit JPEG with unchanged white balance throws <see cref="NotSupportedException"/>.
/// </summary>
public sealed class EmbeddedPreviewDeveloper : IRawDeveloper
{
    private const int HeadBytes = 1 << 20;
    private const int TagCompression = 259;
    private const int TagStripOffsets = 273;
    private const int TagStripByteCounts = 279;
    private const int TagSubIfds = 330;
    private const int TagJpegOffset = 513;
    private const int TagJpegLength = 514;
    private const int MaxIfds = 16;

    /// <inheritdoc />
    public bool IsRaw(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var start = input.CanSeek ? input.Position : -1;
        var head = new byte[HeadBytes];
        var read = 0;
        int n;
        while (read < head.Length && (n = input.Read(head, read, head.Length - read)) > 0)
        {
            read += n;
        }

        if (start >= 0)
        {
            input.Position = start;
        }

        return RawDetector.IsRaw(head.AsSpan(0, read));
    }

    /// <inheritdoc />
    public async Task DevelopAsync(Stream input, Stream output, RawDevelopOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(options);
        if (!options.IsPreviewCompatible)
        {
            throw new NotSupportedException("The embedded preview is the camera's 8-bit JPEG; PNG, 16-bit output and white-balance changes need a native RAW developer.");
        }

        using var copy = new MemoryStream();
        await input.CopyToAsync(copy, cancellationToken).ConfigureAwait(false);
        var bytes = copy.ToArray();
        var preview = FindPreview(bytes) ?? throw new InvalidDataException("No embedded JPEG preview was found in the file.");
        await output.WriteAsync(bytes.AsMemory(preview.Offset, preview.Length), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>The largest embedded JPEG preview, or null.</summary>
    /// <param name="file">The whole RAW file.</param>
    public static (int Offset, int Length)? FindPreview(byte[] file)
    {
        ArgumentNullException.ThrowIfNull(file);
        var block = TiffBlock.Open(file);
        if (block is null || block.U16(2) != 42)
        {
            return null;
        }

        (int Offset, int Length)? best = null;
        var queue = new Queue<int>();
        var seen = new HashSet<int>();
        queue.Enqueue(block.FirstIfd);
        while (queue.Count > 0 && seen.Count < MaxIfds)
        {
            var at = queue.Dequeue();
            if (at <= 0 || at + 2 > file.Length || !seen.Add(at))
            {
                continue;
            }

            var entries = block.Ifd(at);
            Consider(block, entries, file, ref best);
            if (TiffBlock.Find(entries, TagSubIfds) is { } subs)
            {
                for (var i = 0; i < subs.Count && subs.Type is 4 or 13; i++)
                {
                    queue.Enqueue((int)block.U32(subs.At + (i * 4)));
                }
            }

            var next = at + 2 + (block.U16(at) * 12);
            if (next + 4 <= file.Length)
            {
                queue.Enqueue((int)block.U32(next));
            }
        }

        return best;
    }

    private static void Consider(TiffBlock block, List<TiffBlock.Entry> entries, byte[] file, ref (int Offset, int Length)? best)
    {
        long offset = -1;
        long length = -1;
        if (TiffBlock.Find(entries, TagJpegOffset) is { } jo && TiffBlock.Find(entries, TagJpegLength) is { } jl)
        {
            offset = block.U32(jo.At);
            length = block.U32(jl.At);
        }
        else if (TiffBlock.Find(entries, TagCompression) is { } c && block.U16(c.At) is 6 or 7
            && TiffBlock.Find(entries, TagStripOffsets) is { } so && TiffBlock.Find(entries, TagStripByteCounts) is { } sc)
        {
            offset = so.Type == 3 ? block.U16(so.At) : block.U32(so.At);
            length = sc.Type == 3 ? block.U16(sc.At) : block.U32(sc.At);
        }

        if (offset < 0 || length < 4 || offset + length > file.Length)
        {
            return;
        }

        if (file[offset] != 0xFF || file[offset + 1] != 0xD8)
        {
            return;
        }

        if (best is null || length > best.Value.Length)
        {
            best = ((int)offset, (int)length);
        }
    }
}
