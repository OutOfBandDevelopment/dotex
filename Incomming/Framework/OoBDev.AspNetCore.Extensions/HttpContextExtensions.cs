using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace OoBDev.AspNetCore.Extensions;

/// <summary>
/// Generic extensions for HttpContext
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// read the request body from the response
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<string> GetRequestBodyAsync(this HttpContext context)
    {
        var currentPosition = context.Request.Body.Position;
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);

        context.Request.Body.Position = 0;
        var content = await reader.ReadToEndAsync();
        context.Request.Body.Position = currentPosition;
        return content;
    }

    /// <summary>
    /// read the response body from the response
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<string> GetResponseBodyAsync(this HttpContext context)
    {
        var currentPosition = context.Response.Body.Position;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);

        context.Response.Body.Position = 0;
        var content = await reader.ReadToEndAsync();
        context.Response.Body.Position = currentPosition;
        return content;
    }

    /// <summary>
    /// get audit log identifier
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<(string? identifier, string? value)> GetIdentifierAsync(this HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var auditable = endpoint?.Metadata.OfType<AuditRequestAttribute>().FirstOrDefault();
        var controllerAction = endpoint?.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault();

        var parameter =
               controllerAction?.MethodInfo.GetParameters().FirstOrDefault(pi => pi.GetCustomAttribute<FromBodyAttribute>() != null) ??
               controllerAction?.MethodInfo.GetParameters().FirstOrDefault();

        if (string.IsNullOrWhiteSpace(auditable?.Identifier) || controllerAction is null || parameter is null) return default;

        var modelBinderFactory = context.RequestServices.GetRequiredService<IModelBinderFactory>();
        var modelBinderContext = new ModelBinderFactoryContext
        {
            Metadata = new EmptyModelMetadataProvider().GetMetadataForType(parameter.ParameterType),
            BindingInfo = BindingInfo.GetBindingInfo([new FromBodyAttribute()])
        };

        var modelBinder = modelBinderFactory.CreateBinder(modelBinderContext);

        var actionContext = new ActionContext(context, context.GetRouteData(), controllerAction);
        var controllerContext = new ControllerContext(actionContext);
        var modelBindingContext = DefaultModelBindingContext.CreateBindingContext(
            actionContext: actionContext,
            valueProvider: await CompositeValueProvider.CreateAsync(controllerContext),
            metadata: modelBinderContext.Metadata,
            bindingInfo: modelBinderContext.BindingInfo,
            modelName: "");

        await modelBinder.BindModelAsync(modelBindingContext);

        var model = modelBindingContext.Result.IsModelSet ? modelBindingContext.Result.Model : default;
        if (model is not null)
        {
            var property = model.GetType().GetProperty(auditable.Identifier);
            var value = property?.GetValue(model)?.ToString();

            if (value is not null) return (auditable.Identifier, value);
        }

        return default;
    }
}
