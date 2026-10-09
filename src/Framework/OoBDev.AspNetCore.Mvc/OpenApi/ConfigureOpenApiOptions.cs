using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Applies the framework conventions to every named <see cref="OpenApiOptions"/> document: assembly grouping, readable schema ids,
/// application info, permissions, health checks, XML documentation and search query descriptions.
/// </summary>
public class ConfigureOpenApiOptions : IConfigureNamedOptions<OpenApiOptions>
{
    /// <summary>
    /// Configures the options using their own document name.
    /// </summary>
    /// <param name="options">The options.</param>
    public void Configure(OpenApiOptions options) => Configure(options.DocumentName, options);

    /// <summary>
    /// Configures the named document.
    /// </summary>
    /// <param name="name">The document name.</param>
    /// <param name="options">The options to configure.</param>
    public void Configure(string? name, OpenApiOptions options)
    {
        var documentName = name ?? options.DocumentName;

        // controllers are grouped by their assembly name (ApiNamespaceControllerModelConvention); "all" has everything
        options.ShouldInclude = description =>
            string.Equals(documentName, OpenApiDocumentCatalog.AllDocumentName, StringComparison.OrdinalIgnoreCase) || string.Equals(description.GroupName, documentName, StringComparison.OrdinalIgnoreCase);

        // https://wegotcode.com/microsoft/swagger-fix-for-dotnetcore/
        options.CreateSchemaReferenceId = typeInfo =>
            OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo) is null ? null : ResolveSchemaType(typeInfo.Type);

        options.AddDocumentTransformer<ApiInfoDocumentTransformer>();
        options.AddDocumentTransformer<HealthChecksDocumentTransformer>();
        options.AddOperationTransformer<ApplicationPermissionsOperationTransformer>();
        options.AddOperationTransformer<SearchQueryOperationTransformer>();
        options.AddOperationTransformer<XmlDocumentationOperationTransformer>();
        options.AddSchemaTransformer<SearchQuerySchemaTransformer>();
        options.AddSchemaTransformer<XmlDocumentationSchemaTransformer>();
    }

    private static string ResolveSchemaType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsGenericType
            ? $"{type.Namespace}.{type.Name.Split('`')[0]}-{string.Join("_", type.GetGenericArguments().Select(ResolveSchemaType))}"
            : $"{type.Namespace}.{type.Name}";
    }
}
