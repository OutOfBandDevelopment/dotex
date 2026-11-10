namespace OoBDev.WebApi.Models;

/// <summary>An API endpoint and the access it needs.</summary>
public class EndpointRightModel
{
    /// <summary>Gets or sets the HTTP method, or <c>ANY</c> when the action accepts all.</summary>
    public required string Method { get; set; }

    /// <summary>Gets or sets the route template.</summary>
    public required string Route { get; set; }

    /// <summary>Gets or sets the access: <c>anonymous</c>, <c>authenticated</c> or <c>rights</c>.</summary>
    public required string Access { get; set; }

    /// <summary>Gets or sets the application rights, any one of which is enough (empty unless <see cref="Access"/> is <c>rights</c>).</summary>
    public string[] Rights { get; set; } = [];
}
