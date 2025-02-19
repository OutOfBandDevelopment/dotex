namespace OoBDev.AspNetCore.Mvc.Providers.SearchQuery;

/// <summary>
/// Represents the type of HTTP request.
/// </summary>
public enum RequestType
{
    /// <summary>
    /// Indicates a request with JSON data.
    /// </summary>
    Query,

    /// <summary>
    /// Indicates a request with JSON data.
    /// </summary>
    Json,

    /// <summary>
    /// Indicates a request with form data.
    /// </summary>
    Form,
}
