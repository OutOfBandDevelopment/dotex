using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Providers.SearchQuery;

/// <summary>
/// Builds search models based on the incoming HTTP context.
/// </summary>
public class SearchModelBuilder : ISearchModelBuilder
{
    private readonly ISearchModelMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchModelBuilder"/> class.
    /// </summary>
    /// <param name="mapper">The search model mapper.</param>
    public SearchModelBuilder(
        ISearchModelMapper mapper
        ) => _mapper = mapper;

    /// <summary>
    /// Gets the search model asynchronously from the provided HTTP context.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="context">The HTTP context containing the model data.</param>
    /// <param name="modelName">The name of the model in the context.</param>
    /// <returns>A tuple indicating whether the search is successful and the retrieved model.</returns>
    public async Task<(bool isSearch, T? model)> GetModelAsync<T>(HttpContext context, string modelName = "")
        where T : class
    {
        var descriptor = _mapper.GetControllerActionDescriptor(context);
        if (descriptor == null) return (false, default);

        if (!descriptor.MethodInfo.ReturnType.IsAssignableTo(typeof(IQueryable))) return (false, default);

        var actionContext = _mapper.GetActionContext(context, descriptor);
        if (actionContext == null) return (false, default);
        var controllerContext = _mapper.GetControllerContext(actionContext);
        if (controllerContext == null) return (false, default);

        var modelBinderFactory = context.RequestServices.GetRequiredService<IModelBinderFactory>();

        var requestType = _mapper.GetRequestType(context);
        var bindingType = _mapper.GetBindingType(requestType);

        var modelBinderContext = new ModelBinderFactoryContext
        {
            Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(T)),
            BindingInfo = BindingInfo.GetBindingInfo([bindingType])
        };

        var modelBinder = modelBinderFactory.CreateBinder(modelBinderContext);
        var valueProvider = await _mapper.GetValueProviderAsync(context, controllerContext, requestType);

        var modelBindingContext = DefaultModelBindingContext.CreateBindingContext(
            actionContext: actionContext,
            valueProvider: valueProvider,
            metadata: modelBinderContext.Metadata,
            bindingInfo: modelBinderContext.BindingInfo,
            modelName: modelName);

        await modelBinder.BindModelAsync(modelBindingContext);

        return modelBindingContext.Result.IsModelSet &&
            modelBindingContext.Result.Model is T model
            ? ((bool isSearch, T? model))(true, model)
            : ((bool isSearch, T? model))(true, default);
    }
}
