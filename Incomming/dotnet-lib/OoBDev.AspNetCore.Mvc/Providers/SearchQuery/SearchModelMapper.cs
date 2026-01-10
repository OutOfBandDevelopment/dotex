using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Providers.SearchQuery;

/// <summary>
/// Maps search-related model information from the HTTP context.
/// </summary>
public class SearchModelMapper : ISearchModelMapper
{
    /// <summary>
    /// Retrieves the controller action descriptor from the provided HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The controller action descriptor if found; otherwise, null.</returns>
    public ControllerActionDescriptor? GetControllerActionDescriptor(HttpContext context) =>
        context.GetEndpoint()?.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault();

    /// <summary>
    /// Creates an action context from the provided HTTP context and controller action descriptor.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="descriptor">The controller action descriptor.</param>
    /// <returns>The created action context if both parameters are valid; otherwise, null.</returns>
    public ActionContext? GetActionContext(HttpContext context, ControllerActionDescriptor descriptor) =>
         descriptor switch
         {
             ControllerActionDescriptor => new ActionContext(context, context.GetRouteData(), descriptor),
             _ => null
         };

    /// <summary>
    /// Creates a controller context from the provided action context.
    /// </summary>
    /// <param name="context">The action context.</param>
    /// <returns>The created controller context if the action context is valid; otherwise, null.</returns>
    public ControllerContext? GetControllerContext(ActionContext? context) =>
        context switch
        {
            ActionContext actionContext => new ControllerContext(actionContext),
            _ => null
        };

    /// <summary>
    /// Determines the type of request based on the provided HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The type of request.</returns>
    public RequestType GetRequestType(HttpContext context) => context.Request switch
    {
        HttpRequest request when request.HasFormContentType => RequestType.Form,
        HttpRequest request when request.HasJsonContentType() => RequestType.Json,
        HttpRequest request when request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) => RequestType.Json,
        _ => RequestType.Query,
    };

    /// <summary>
    /// Retrieves the binding type metadata based on the request type.
    /// </summary>
    /// <param name="requestType">The type of request.</param>
    /// <returns>The binding type metadata.</returns>
    public IBindingSourceMetadata GetBindingType(RequestType requestType) => requestType switch
    {
        RequestType.Form => new FromFormAttribute(),
        RequestType.Json => new FromBodyAttribute(),
        RequestType.Query => new FromQueryAttribute(),
        _ => throw new NotSupportedException($"{nameof(requestType)}: {requestType} is not supported"),
    };

    /// <summary>
    /// Retrieves the value provider asynchronously based on the request type.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="controllerContext">The controller context.</param>
    /// <param name="requestType">The type of request.</param>
    /// <returns>The value provider.</returns>
    public async Task<IValueProvider> GetValueProviderAsync(
        HttpContext context,
        ControllerContext controllerContext,
        RequestType requestType
        ) => requestType switch
        {
            //TODO: dictionary <string, object> does not map for forms and query string
            RequestType.Form => new FormValueProvider(BindingSource.Form, context.Request.Form, null),
            RequestType.Json => await CompositeValueProvider.CreateAsync(controllerContext),
            RequestType.Query => new QueryStringValueProvider(BindingSource.Query, context.Request.Query, null),
            _ => throw new NotSupportedException($"{nameof(requestType)}: {requestType} is not supported"),
        };

}
