using OoBDev.SemanticKernel;
using Microsoft.AspNetCore.Http;

namespace OoBDev.AspNetCore.Extensions;

public class HttpModelNameAccessor : IModelNameAccessor
{
    private readonly IHttpContextAccessor _contextAccessor;

    public HttpModelNameAccessor(
        IHttpContextAccessor contextAccessor
        )
    {
        _contextAccessor = contextAccessor;
    }

    private string? _modelName;

    public string? ModelName
    {
        get => string.IsNullOrWhiteSpace(_modelName) ? _contextAccessor.HttpContext?.Request.Query["model"] : _modelName;
        set => _modelName = value;
    }
}
