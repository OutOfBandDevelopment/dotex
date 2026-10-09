using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Schema transformer that copies type and property XML documentation into schema descriptions.
/// </summary>
public class XmlDocumentationSchemaTransformer(XmlDocumentationProvider documentation) : IOpenApiSchemaTransformer
{
    /// <summary>
    /// Applies the XML documentation of the type or property to the schema.
    /// </summary>
    /// <param name="schema">The schema to modify.</param>
    /// <param name="context">The transformer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        MemberInfo? member = context.JsonPropertyInfo?.AttributeProvider as MemberInfo ?? (context.JsonPropertyInfo is null ? context.JsonTypeInfo.Type : null);
        if (member != null)
        {
            schema.Description ??= documentation.GetSummary(member);
        }

        return Task.CompletedTask;
    }
}
