using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Providers.SearchQuery;

/// <summary>
/// Maps search-related model information from the HTTP context.
/// </summary>
public interface ISearchModelMapper
{
    /// <summary>
    /// Retrieves the controller action descriptor from the provided HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The controller action descriptor if found; otherwise, null.</returns>
    ControllerActionDescriptor? GetControllerActionDescriptor(HttpContext context);

    /// <summary>
    /// Creates an action context from the provided HTTP context and controller action descriptor.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="descriptor">The controller action descriptor.</param>
    /// <returns>The created action context if both parameters are valid; otherwise, null.</returns>
    ActionContext? GetActionContext(HttpContext context, ControllerActionDescriptor descriptor);

    /// <summary>
    /// Creates a controller context from the provided action context.
    /// </summary>
    /// <param name="context">The action context.</param>
    /// <returns>The created controller context if the action context is valid; otherwise, null.</returns>
    ControllerContext? GetControllerContext(ActionContext? context);

    /// <summary>
    /// Determines the type of request based on the provided HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The type of request.</returns>
    RequestType GetRequestType(HttpContext context);

    /// <summary>
    /// Retrieves the binding type metadata based on the request type.
    /// </summary>
    /// <param name="requestType">The type of request.</param>
    /// <returns>The binding type metadata.</returns>
    IBindingSourceMetadata GetBindingType(RequestType requestType);

    /// <summary>
    /// Retrieves the value provider asynchronously based on the request type.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="controllerContext">The controller context.</param>
    /// <param name="requestType">The type of request.</param>
    /// <returns>The value provider.</returns>
    Task<IValueProvider> GetValueProviderAsync(HttpContext context, ControllerContext controllerContext, RequestType requestType);

}
