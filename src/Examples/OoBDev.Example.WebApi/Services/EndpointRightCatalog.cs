using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.WebApi.Services;

/// <summary>Describes who can call each API action from its authorization attributes.</summary>
public class EndpointRightCatalog
{
    /// <summary>Lists the actions that have a route, with the access each one needs.</summary>
    /// <param name="actions">The action descriptors of the host.</param>
    /// <returns>One row per action and HTTP method, ordered by route.</returns>
    public IReadOnlyList<EndpointRightModel> Describe(IEnumerable<ActionDescriptor> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        return
        [
            .. actions
                .Where(action => action.AttributeRouteInfo?.Template is not null)
                .SelectMany(Rows)
                .OrderBy(row => row.Route, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.Method, StringComparer.Ordinal),
        ];
    }

    private static IEnumerable<EndpointRightModel> Rows(ActionDescriptor action)
    {
        var metadata = action.EndpointMetadata ?? [];
        var rights = metadata.OfType<ApplicationRightAttribute>().SelectMany(a => a.Rights).Distinct(StringComparer.Ordinal).ToArray();
        var access = metadata.OfType<IAllowAnonymous>().Any() ? "anonymous"
            : rights.Length > 0 ? "rights"
            : metadata.OfType<IAuthorizeData>().Any() ? "authenticated"
            : "anonymous";
        var methods = action.ActionConstraints?.OfType<HttpMethodActionConstraint>().SelectMany(c => c.HttpMethods).Distinct().ToArray() ?? [];
        foreach (var method in methods.Length == 0 ? ["ANY"] : methods)
            yield return new EndpointRightModel
            {
                Method = method,
                Route = "/" + action.AttributeRouteInfo!.Template!.TrimStart('/'),
                Access = access,
                Rights = access == "rights" ? rights : [],
            };
    }
}
