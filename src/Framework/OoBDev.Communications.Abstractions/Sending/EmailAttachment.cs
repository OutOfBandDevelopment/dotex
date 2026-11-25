using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>The plain implementation of <see cref="IEmailAttachment"/>; create it with <see cref="FromBytes"/>, <see cref="FromMemory"/> or <see cref="FromFile"/>.</summary>
public sealed class EmailAttachment : IEmailAttachment
{
    private readonly ReadOnlyMemory<byte> _memory;
    private readonly string? _path;

    private EmailAttachment(string fileName, string contentType, string? contentId, ReadOnlyMemory<byte> memory, string? path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        FileName = fileName;
        ContentType = contentType;
        ContentId = contentId;
        _memory = memory;
        _path = path;
    }

    /// <inheritdoc />
    public string FileName { get; }

    /// <inheritdoc />
    public string ContentType { get; }

    /// <inheritdoc />
    public string? ContentId { get; }

    /// <summary>Creates an attachment that owns a byte array.</summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The media type.</param>
    /// <param name="content">The content; not copied.</param>
    /// <param name="contentId">The inline content id, or <c>null</c>.</param>
    /// <returns>The attachment.</returns>
    public static EmailAttachment FromBytes(string fileName, string contentType, byte[] content, string? contentId = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        return new(fileName, contentType, contentId, content, null);
    }

    /// <summary>Creates an attachment over a memory block without copying it; the caller keeps the block alive until the send finishes.</summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The media type.</param>
    /// <param name="content">The content.</param>
    /// <param name="contentId">The inline content id, or <c>null</c>.</param>
    /// <returns>The attachment.</returns>
    public static EmailAttachment FromMemory(string fileName, string contentType, ReadOnlyMemory<byte> content, string? contentId = null)
        => new(fileName, contentType, contentId, content, null);

    /// <summary>Creates an attachment read from a file when the message is built; use a file from <c>ITempFileFactory</c> for generated content.</summary>
    /// <param name="fileName">The file name shown to the recipient.</param>
    /// <param name="contentType">The media type.</param>
    /// <param name="path">The file to read; it must exist until the send finishes.</param>
    /// <param name="contentId">The inline content id, or <c>null</c>.</param>
    /// <returns>The attachment.</returns>
    public static EmailAttachment FromFile(string fileName, string contentType, string path, string? contentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new(fileName, contentType, contentId, default, path);
    }

    /// <inheritdoc />
    public async ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
        => _path is null ? _memory : await File.ReadAllBytesAsync(_path, cancellationToken).ConfigureAwait(false);
}
