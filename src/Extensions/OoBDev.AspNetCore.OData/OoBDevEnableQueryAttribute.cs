using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OoBDev.System.ComponentModel.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OoBDev.AspNetCore.OData;

/// <summary>
/// <c>[EnableQuery]</c> with the configured <see cref="ODataQueryLimits"/>, the search annotations
/// (<see cref="NotFilterableAttribute"/>, <see cref="NotSortableAttribute"/>) and RFC 9457 errors.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class OoBDevEnableQueryAttribute : EnableQueryAttribute
{
    /// <summary>The response header name.</summary>
    public const string AcceptQueryHeader = "Accept-Query";

    /// <inheritdoc />
    public override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
    {
        ArgumentNullException.ThrowIfNull(actionExecutingContext);
        var limits = LimitsOf(actionExecutingContext.HttpContext);
        PageSize = limits.DefaultPageSize;
        MaxNodeCount = limits.MaxNodeCount;
        MaxExpansionDepth = limits.AllowExpand ? limits.MaxExpansionDepth : 0;
        AllowedQueryOptions = AllowedQueryOptions.Filter | AllowedQueryOptions.OrderBy | AllowedQueryOptions.Top
            | AllowedQueryOptions.Skip | AllowedQueryOptions.Select | AllowedQueryOptions.Count
            | (limits.AllowExpand ? AllowedQueryOptions.Expand : AllowedQueryOptions.None);

        actionExecutingContext.HttpContext.Response.Headers[AcceptQueryHeader] = "\"application/x-www-form-urlencoded\"";
        base.OnActionExecuting(actionExecutingContext);
        ToProblem(actionExecutingContext.Result is ObjectResult r ? r : null, result => actionExecutingContext.Result = result);
    }

    /// <inheritdoc />
    public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
    {
        ArgumentNullException.ThrowIfNull(queryOptions);
        var elementType = queryOptions.Context.ElementClrType;

        if (queryOptions.Filter is not null)
            RejectHidden(queryOptions.RawValues.Filter, HiddenNames<NotFilterableAttribute>(elementType, a => a.TargetName), "$filter");
        if (queryOptions.OrderBy is not null)
            RejectHidden(queryOptions.RawValues.OrderBy, HiddenNames<NotSortableAttribute>(elementType, a => a.TargetName), "$orderby");

        var limits = LimitsOf(request.HttpContext);
        queryOptions.Validate(new ODataValidationSettings
        {
            MaxTop = limits.MaxTop,
            MaxNodeCount = limits.MaxNodeCount,
            MaxExpansionDepth = limits.AllowExpand ? limits.MaxExpansionDepth : 0,
            AllowedQueryOptions = AllowedQueryOptions.Filter | AllowedQueryOptions.OrderBy | AllowedQueryOptions.Top
                | AllowedQueryOptions.Skip | AllowedQueryOptions.Select | AllowedQueryOptions.Count
                | (limits.AllowExpand ? AllowedQueryOptions.Expand : AllowedQueryOptions.None),
        });
    }

    private static ODataQueryLimits LimitsOf(HttpContext context) =>
        context.RequestServices.GetService<IOptions<ODataQueryLimits>>()?.Value ?? new ODataQueryLimits();

    /// <inheritdoc />
    public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
    {
        ArgumentNullException.ThrowIfNull(actionExecutedContext);
        base.OnActionExecuted(actionExecutedContext);
        ToProblem(actionExecutedContext.Result as ObjectResult, result => actionExecutedContext.Result = result);

        if (actionExecutedContext.Result is ObjectResult { Value: IEnumerable items and not string } ok
            && actionExecutedContext.HttpContext.ODataFeature().TotalCount is { } total)
        {
            ok.Value = new Dictionary<string, object?> { ["@odata.count"] = total, ["value"] = items };
        }
    }

    private static void ToProblem(ObjectResult? result, Action<ObjectResult> replace)
    {
        if (result is not { StatusCode: StatusCodes.Status400BadRequest })
            return;
        var detail = result.Value switch
        {
            string text => text,
            Microsoft.OData.ODataError error => error.Message,
            _ => "The query specified in the URI is not valid.",
        };
        replace(Problem(detail));
    }

    private static ObjectResult Problem(string detail) => new(new ProblemDetails
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "The OData query is not valid.",
        Detail = detail,
    })
    {
        StatusCode = StatusCodes.Status400BadRequest,
        ContentTypes = { "application/problem+json" },
    };

    private static HashSet<string> HiddenNames<TAttribute>(Type type, Func<TAttribute, string?> target)
        where TAttribute : Attribute
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetCustomAttributes<TAttribute>(true).Any())
                names.Add(property.Name);
        }
        foreach (var attribute in type.GetCustomAttributes<TAttribute>(true))
        {
            if (target(attribute) is { Length: > 0 } name)
                names.Add(name);
        }
        return names;
    }

    private static void RejectHidden(string? raw, HashSet<string> hidden, string option)
    {
        if (string.IsNullOrEmpty(raw) || hidden.Count == 0)
            return;
        foreach (var name in hidden)
        {
            if (Regex.IsMatch(raw, $@"(?<![\w]){Regex.Escape(name)}(?![\w])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
                throw new Microsoft.OData.ODataException($"The property '{name}' cannot be used in {option}.");
        }
    }
}
