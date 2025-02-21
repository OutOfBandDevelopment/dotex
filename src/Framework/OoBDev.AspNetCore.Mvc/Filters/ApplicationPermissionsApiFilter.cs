using OoBDev.AspNetCore.Mvc.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace OoBDev.AspNetCore.Mvc.Filters;

/// <summary>
/// operation filter to extend swagger to include application rights 
/// </summary>
public class ApplicationPermissionsApiFilter : IOperationFilter
{
    /// <summary>
    /// Applies the operation filter to include application rights in Swagger documentation.
    /// </summary>
    /// <param name="operation">The OpenApiOperation to be modified.</param>
    /// <param name="context">The OperationFilterContext providing information about the operation.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var allowAnonymous =
            context.MethodInfo.GetCustomAttributes<AllowAnonymousAttribute>()
            .Concat(
            context.MethodInfo.DeclaringType?.GetCustomAttributes<AllowAnonymousAttribute>() ??
                []
            ).Any();

        var applicationRights =
            context.MethodInfo.GetCustomAttributes<ApplicationRightAttribute>()
            .Concat(
            context.MethodInfo.DeclaringType?.GetCustomAttributes<ApplicationRightAttribute>() ??
                []
            ).SelectMany(a => a.Rights);

        //operation.Security.Add(new OpenApiSecurityRequirement()
        //{
        //    { new OpenApiSecurityScheme()  { Name="Application Rights" }, applicationRights.ToList() }
        //});

        operation.Extensions.Add("x-permissions", new ApiPermissionsExtension(allowAnonymous, applicationRights));
    }
}
