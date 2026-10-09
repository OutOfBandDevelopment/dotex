using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OoBDev.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Operation transformer that adds the <c>x-permissions</c> extension (anonymous access and required application rights) to every operation.
/// </summary>
public class ApplicationPermissionsOperationTransformer : IOpenApiOperationTransformer
{
    /// <summary>
    /// Adds the <see cref="ApiPermissionsExtension"/> to the operation.
    /// </summary>
    /// <param name="operation">The operation to modify.</param>
    /// <param name="context">The transformer context describing the endpoint.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // endpoint metadata already merges the action and controller level attributes
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var allowAnonymous = metadata.OfType<AllowAnonymousAttribute>().Any();
        var applicationRights = metadata.OfType<ApplicationRightAttribute>().SelectMany(a => a.Rights);

        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        operation.Extensions["x-permissions"] = new ApiPermissionsExtension(allowAnonymous, applicationRights);
        return Task.CompletedTask;
    }
}
