namespace OoBDev.WebApi.Models;

/// <summary>A document stored in the example blob container.</summary>
public class DocumentInfoModel
{
    /// <summary>Gets or sets the stored file name (the path inside the container).</summary>
    public required string FileName { get; set; }

    /// <summary>Gets or sets the content type.</summary>
    public required string ContentType { get; set; }
}
