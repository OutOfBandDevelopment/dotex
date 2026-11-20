namespace OoBDev.WebApi.Models;

/// <summary>A query to run.</summary>
public class DataQueryRequestModel
{
    /// <summary>Gets or sets the T-SQL text; it must be exactly one SELECT.</summary>
    public string Sql { get; set; } = string.Empty;
}
