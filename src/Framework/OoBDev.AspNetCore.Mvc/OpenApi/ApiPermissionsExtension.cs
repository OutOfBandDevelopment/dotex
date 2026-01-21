using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Declare permissions required for application endpoint
/// </summary>
/// <param name="allowAnonymous">End point allows unauthenticated requests</param>
/// <param name="rights">end point requires at least one of these permissions</param>
public class ApiPermissionsExtension(bool allowAnonymous, IEnumerable<string> rights) : IOpenApiExtension
{
    /// <summary>
    /// End point allows unauthenticated requests
    /// </summary>
    public bool AllowAnonymous { get; } = allowAnonymous;

    /// <summary>
    /// end point requires at least one of these permissions
    /// </summary>
    public IReadOnlyCollection<string> Rights { get; } = rights.Distinct().ToList().AsReadOnly();

    /// <summary>
    /// Writes the required permissions to the OpenAPI writer.
    /// </summary>
    /// <param name="writer">The OpenAPI writer to write to.</param>
    /// <param name="specVersion">The OpenAPI specification version.</param>
    public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        writer.WriteStartObject();
        writer.WriteProperty("anonymous", AllowAnonymous);
        writer.WriteOptionalCollection("right", Rights, (w, v) => w.WriteValue(v!));
        writer.WriteEndObject();
    }
}
