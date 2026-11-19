namespace OoBDev.Documents.Containers;

/// <summary>Settings of the file system blob container provider, bound from the <c>FileSystemBlobOptions</c> section.</summary>
public sealed class FileSystemBlobOptions
{
    /// <summary>Gets or sets the root directory; each container is a sub directory of it. The provider is not offered while this is empty.</summary>
    public string? RootDirectory { get; set; }
}
