namespace OoBDev.WebApi.Models;

/// <summary>New text for a listed template file.</summary>
public class TemplateSaveRequestModel
{
    /// <summary>Gets or sets the file name from the template list.</summary>
    public required string File { get; set; }

    /// <summary>Gets or sets the new template text.</summary>
    public required string Text { get; set; }
}
