# Event Auditing

Out-of-Band Development - Dotnet Extensions

## Summary

I would like a way to tag classes or methods in with auditing data. Logging middlware would can the stackframe for instances of these values allowing for additional data to be captured and added to the logging events transparently. this would allow for business context to be added to a structured logs without having to capture the information at the point of loggin.

### Examples

```
[AuditRequest(LogTypeName="Special Log", Identifer="special value")]
public void MyMethod(){
    ...
}

```

Having a method setup like this would add the information above to the log... optionally the entire stack could be scanned or it could just be the first instance or the N-first instances.

## Attributes


/// <summary>
/// When this action is called the request/response will be captured
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class AuditRequestAttribute : Attribute
{
    public const string DefaultLogTypeName = "Application Audit";

    /// <summary>
    /// Optional: name of the property to use for Identifier and IdentifierValue
    /// </summary>
    public string? Identifier { get; set; }
    public string? LogTypeName { get; set; } = DefaultLogTypeName;
    public bool ExcludeRequest { get; set; }
    public bool ExcludeResponse { get; set; }
}


## Models

this is just a proposal, maybe clean it up like put the time stamp on the base object and use the request/corrleation values

### 

[ConnectionStringName("Logs")]
[StoredProcedure("[Log].[ins_AudiLogData]")]
public record AuditLogEntry
{
    //Needs entry for ApplicationName
    [StringLength(50)]
    public required string ApplicationName { get; init; }

    [StringLength(50)]
    public required string LogTypeName { get; init; }

    /// <summary>
    /// Note: this is a session correlation id
    /// </summary>
    [StringLength(36)]
    public required string ApplicationSessionID { get; init; }

    /// <summary>
    /// Note: this should be HttpContext.Request.Path
    /// </summary>
    [StringLength(255)]
    public string? Description { get; init; }

    /// <summary>
    /// Note: this is the name of the property used for Primary Input
    /// </summary>
    [StringLength(36)]
    public string? Identifier { get; init; } = string.Empty;

    /// <summary>
    /// Note: this is the value of the property used for Primary Input
    /// </summary>
    [StringLength(50)]
    public string? IdentifierValue { get; init; } = string.Empty;

    /// <summary>
    /// Note: Request.HttpContext.Connection.RemoteIpAddress.ToString(),
    /// </summary>
    [StringLength(50)]
    public string? IPAddress { get; init; }

    /// <summary>
    /// json payload for description
    /// </summary>
    public required AuditLogInfo AuditInfo { get; init; }
}

public record class AuditLogInfo
{
    public required string ServiceCall { get; init; }
    public object? Parameters { get; init; }
    public object? Response { get; init; }
    public required DateTimeOffset TimeStamp { get; init; }
    public string? RequestId { get; init; }
    public string? CorrelationId { get; init; }
    public int StatusCode { get; init; }
}


## Logger

public class ContextualLogger : ILogger
{
    private readonly ILogger _inner;

    public ContextualLogger(ILogger inner)
    {
        _inner = inner;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        var message = $"[MyContext] {formatter(state, exception)}";
        _inner.Log(logLevel, eventId, message, exception, (_, __) => message);
    }

    public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);
    public IDisposable BeginScope<TState>(TState state) => _inner.BeginScope(state);
}

## HTTP Middleware

public class LoggingContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingContextMiddleware> _logger;

    public LoggingContextMiddleware(RequestDelegate next, ILogger<LoggingContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = context.TraceIdentifier,
            ["Path"] = context.Request.Path
        }))
        {
            await _next(context);
        }
    }
}

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