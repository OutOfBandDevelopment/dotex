namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// Options for <see cref="ProblemDetailsExceptionHandler"/> (section <c>OoBDev:ProblemDetails</c>).
/// </summary>
public class ProblemDetailsExceptionHandlerOptions
{
    /// <summary>
    /// Gets or initializes whether the response names the original error location (exception type, method, file and line of the top stack frame).
    /// Off by default because it discloses internals; turn it on for development and test environments.
    /// </summary>
    public bool IncludeErrorLocation { get; init; }
}
