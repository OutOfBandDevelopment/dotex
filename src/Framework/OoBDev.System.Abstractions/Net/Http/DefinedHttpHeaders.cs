namespace OoBDev.System.Net.Http;

/// <summary>
/// Contains constant values for commonly used HTTP headers.
/// </summary>
public static class DefinedHttpHeaders
{
    /// <summary>
    /// Represents the "X-Correlation-ID" HTTP header used for correlation identification.
    /// </summary>
    public const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Represents the "X-Request-ID" HTTP header used for request identification.
    /// </summary>
    public const string RequestIdHeader = "X-Request-ID";

    /// <summary>
    /// Represents the "Accept-Language" HTTP header used for specifying acceptable languages for the response.
    /// </summary>
    public const string AcceptLanguage = "Accept-Language";

    /// <summary>
    /// Represents the "Content-Language" HTTP header used for specifying the language of the content.
    /// </summary>
    public const string ContentLanguage = "Content-Language";
}
