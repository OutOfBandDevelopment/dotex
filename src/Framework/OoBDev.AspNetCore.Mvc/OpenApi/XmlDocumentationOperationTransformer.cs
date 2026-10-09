using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Operation transformer that copies controller action XML documentation into the operation summary, description and parameter descriptions.
/// </summary>
public class XmlDocumentationOperationTransformer(XmlDocumentationProvider documentation) : IOpenApiOperationTransformer
{
    /// <summary>
    /// Applies the XML documentation of the action to the operation.
    /// </summary>
    /// <param name="operation">The operation to modify.</param>
    /// <param name="context">The transformer context describing the endpoint.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor action) return Task.CompletedTask;

        operation.Summary ??= documentation.GetSummary(action.MethodInfo);
        operation.Description ??= documentation.GetRemarks(action.MethodInfo);

        foreach (var parameter in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
        {
            if (parameter.Name != null)
            {
                parameter.Description ??= documentation.GetParameter(action.MethodInfo, parameter.Name);
            }
        }

        return Task.CompletedTask;
    }
}
