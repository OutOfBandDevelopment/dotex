using System.ComponentModel.DataAnnotations;

namespace OoBDev.Microsoft.Azure.AISearch;

/// <summary>
/// Settings of the Azure AI Search vector store.
/// </summary>
public class AzureAISearchOptions
{
    /// <summary>
    /// Gets or sets the service endpoint, for example <c>https://name.search.windows.net</c>.
    /// </summary>
    [Required]
    public required string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the default index name.
    /// </summary>
    [Required]
    public required string IndexName { get; set; }

    /// <summary>
    /// Gets or sets the admin key; when empty, <c>DefaultAzureCredential</c> is used.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the index is created from the first vector length when it is missing.
    /// </summary>
    public bool EnsureIndexExists { get; set; }

    /// <summary>
    /// Gets or sets the number of results a neighbor search returns.
    /// </summary>
    public int Top { get; set; } = 50;
}
