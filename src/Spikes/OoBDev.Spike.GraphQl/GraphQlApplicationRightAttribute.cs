using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.AspNetCore.Authorization;

namespace OoBDev.Spike.GraphQl;

/// <summary>The GraphQL twin of <c>[ApplicationRight]</c>: the same checker, run as field middleware.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class GraphQlApplicationRightAttribute(params string[] rights) : ObjectFieldDescriptorAttribute
{
    public const string PrincipalKey = "principal";

    protected override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
    {
        descriptor.Use(next => async middlewareContext =>
        {
            var principal = middlewareContext.ContextData.TryGetValue(PrincipalKey, out var value) ? value as ClaimsPrincipal : null;
            var result = principal is null
                ? RightCheckResult.Unauthenticated
                : middlewareContext.Services.GetRequiredService<IApplicationRightChecker>().Check(principal, rights);
            if (result != RightCheckResult.Allowed)
            {
                middlewareContext.ReportError(ErrorBuilder.New()
                    .SetMessage(result == RightCheckResult.Unauthenticated ? "Unauthenticated." : "Forbidden.")
                    .SetCode(result == RightCheckResult.Unauthenticated ? "AUTH_NOT_AUTHENTICATED" : "AUTH_FORBIDDEN")
                    .Build());
                middlewareContext.Result = null;
                return;
            }

            await next(middlewareContext).ConfigureAwait(false);
        });
    }
}
