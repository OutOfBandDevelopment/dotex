using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>A file sent with an email. The content is not a stream: it is a file on disk, a byte array or a memory block.</summary>
public interface IEmailAttachment
{
    /// <summary>Gets the file name shown to the recipient.</summary>
    string FileName { get; }

    /// <summary>Gets the media type, for example <c>application/pdf</c>.</summary>
    string ContentType { get; }

    /// <summary>Gets the content id for inline use (<c>cid:</c> in the HTML body), or <c>null</c> for a normal attachment.</summary>
    string? ContentId { get; }

    /// <summary>Gets the content; a file-backed attachment reads the file when called.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The bytes of the attachment.</returns>
    ValueTask<global::System.ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default);
}
