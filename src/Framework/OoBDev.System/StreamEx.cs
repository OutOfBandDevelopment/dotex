using System.IO;
using System.Threading.Tasks;

namespace OoBDev.System;

/// <summary>
/// extensions for <c>System.IO.Stream</c>
/// </summary>
public static class StreamEx
{
    /// <summary>
    /// simple wrapper to get stream content as string
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<string?> ReadAsStringAsync(this Stream? stream)
    {
        if (stream == null) return null;
        using var sr = new StreamReader(stream); //TODO: should this leave the underlying stream open?
        return await sr.ReadToEndAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads all bytes from a stream and returns them as a byte array.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>A byte array containing the stream contents, or null if the stream is null.</returns>
    public static async Task<byte[]?> AsBytesAsync(this Stream? stream)
    {
        if (stream == null) return null;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms).ConfigureAwait(false);
        return ms.ToArray();
    }

    /// <summary>
    /// Synchronously reads all bytes from a stream and returns them as a byte array.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>A byte array containing the stream contents, or null if the stream is null.</returns>
    public static byte[]? AsBytes(this Stream? stream)
    {
        if (stream == null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
