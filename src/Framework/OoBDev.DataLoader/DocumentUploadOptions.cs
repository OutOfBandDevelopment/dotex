namespace OoBDev.DataLoader;

/// <summary>Column names used by <see cref="DocumentUploadHandler"/>.</summary>
public sealed class DocumentUploadOptions
{
    /// <summary>Column with the file to upload, relative to the data folder.</summary>
    public string FileColumn { get; set; } = "__SourceFilePath";

    /// <summary>Column with the content type of the file; <c>application/octet-stream</c> when empty.</summary>
    public string ContentTypeColumn { get; set; } = "__SourceFileContent";

    /// <summary>Column that receives the document key; a row that already has one is not uploaded again.</summary>
    public string KeyColumn { get; set; } = "StorageKey";

    /// <summary>Column set to the file name when it is empty; null leaves it alone.</summary>
    public string? NameColumn { get; set; } = "Name";
}
