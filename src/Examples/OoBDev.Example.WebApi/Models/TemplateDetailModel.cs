namespace OoBDev.WebApi.Models;

/// <summary>A template with its source text.</summary>
public class TemplateDetailModel
{
    /// <summary>Gets or sets the template description.</summary>
    public required TemplateInfoModel Info { get; set; }

    /// <summary>Gets or sets the template source text.</summary>
    public required string Text { get; set; }
}
