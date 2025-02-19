using System;

namespace OoBDev.System.IO;

/// <summary>
/// Represents metadata associated with a file.
/// </summary>
public readonly ref struct FileMetaData
{
    /// <summary>
    /// Gets the universally unique identifier (UUID) of the file.
    /// </summary>
    public string Uuid { get; init; }

    /// <summary>
    /// Gets the path of the file.
    /// </summary>
    public string Path { get; init; }

    /// <summary>
    /// Gets the hash value of the file.
    /// </summary>
    public ReadOnlySpan<byte> Hash { get; init; }

    /// <summary>
    /// Gets the base path of the file.
    /// </summary>
    public string BasePath { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileMetaData"/> record.
    /// </summary>
    /// <param name="uuid">The universally unique identifier (UUID) of the file.</param>
    /// <param name="path">The path of the file.</param>
    /// <param name="hash">The hash value of the file.</param>
    /// <param name="basePath">The base path of the file.</param>
    public FileMetaData(string uuid, string path, ReadOnlySpan<byte> hash, string basePath)
    {
        Uuid = uuid;
        Path = path;
        Hash = hash;
        BasePath = basePath;
    }
}
