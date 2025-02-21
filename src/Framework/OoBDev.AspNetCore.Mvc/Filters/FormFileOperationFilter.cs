using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.Filters;

/// <summary>
/// Implementation of <see cref="IOperationFilter"/> for handling file upload in Swagger UI.
/// </summary>
public class FormFileOperationFilter : IOperationFilter
{
    // Reference: https://dejanstojanovic.net/aspnet/2021/april/handling-file-upload-in-aspnet-core-5-with-swagger-ui/?theme=light

    /// <summary>
    /// Applies the file upload operation filter to modify the Swagger UI documentation.
    /// </summary>
    /// <param name="operation">The OpenApiOperation representing the Swagger operation.</param>
    /// <param name="context">The OperationFilterContext providing information about the operation.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // MIME type for file uploads
        var fileUploadMime = "multipart/form-data";

        // If the operation doesn't have a request body or doesn't support file uploads, return
        if (operation.RequestBody == null || !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))
            return;

        // Get parameters of type IFormFile from the method
        var fileParams = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile));

        // Map IFormFile parameters to OpenApiSchema properties
        operation.RequestBody.Content[fileUploadMime].Schema.Properties =
            fileParams.ToDictionary(k => k.Name ?? k.ParameterType.Name, v => new OpenApiSchema()
            {
                Type = "string",
                Format = "binary"
            });
    }
}
