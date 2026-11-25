namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// Present on requests that <see cref="QueryVerbMiddleware"/> routed as <c>GET</c>.
/// </summary>
public interface IQueryVerbFeature
{
    /// <summary>
    /// The method the client sent: <c>QUERY</c>, or <c>POST</c> when the override header was used.
    /// </summary>
    string OriginalMethod { get; }
}
