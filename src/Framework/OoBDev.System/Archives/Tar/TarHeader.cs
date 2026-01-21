namespace OoBDev.System.Archives.Tar;

/// <summary>
/// Represents the header information for a file entry in a TAR archive.
/// </summary>
public record TarHeader
{
    /// <summary>
    /// Gets the name of the file in the archive.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the file permission mode in octal format.
    /// </summary>
    public required string FileMode { get; init; }

    /// <summary>
    /// Gets the numeric user ID of the file owner.
    /// </summary>
    public required string OwnerId { get; init; }

    /// <summary>
    /// Gets the numeric group ID of the file owner.
    /// </summary>
    public required string GroupId { get; init; }

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    public required int FileSize { get; init; }

    /// <summary>
    /// Gets the last modification time of the file as a Unix timestamp.
    /// </summary>
    public required int LastModifiedTime { get; init; }

    /// <summary>
    /// Gets the header checksum value.
    /// </summary>
    public required string CheckSum { get; init; }

    /// <summary>
    /// Gets the type of file entry.
    /// </summary>
    public required TarFileType FileType { get; init; }

    /// <summary>
    /// Gets the name of the linked file for hard links or symbolic links.
    /// </summary>
    public required string LinkedFile { get; init; }
}
