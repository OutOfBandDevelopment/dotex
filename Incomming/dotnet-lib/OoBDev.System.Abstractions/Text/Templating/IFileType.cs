namespace OoBDev.System.Text.Templating;

/// <summary>
/// This interface allows objects that implement it to provide information about a specific file type, 
/// including its extension, content type, and whether it is considered a template type.
/// </summary>
public interface IFileType
{
    /// <summary>
    /// Gets the file extension.
    /// </summary>
    string Extension { get; }

    /// <summary>
    /// Gets the content type of the file.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Gets a value indicating whether the file type is considered a template type.
    /// </summary>
    bool IsTemplateType { get; }
}
