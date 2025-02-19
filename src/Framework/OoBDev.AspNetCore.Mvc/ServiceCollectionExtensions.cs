using OoBDev.AspNetCore.Mvc.Authorization;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.AspNetCore.Mvc.Providers.SearchQuery;
using OoBDev.AspNetCore.Mvc.SwaggerGen;
using OoBDev.Extensions;
using OoBDev.System.Linq.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Globalization;
using System.Security.Claims;

namespace OoBDev.AspNetCore.Mvc;

/// <summary>
/// Extension methods for configuring ASP.Net Core extensions and related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds IOC configurations to support all ASP.Net Core extensions provided by this library.
    /// </summary>
    /// <param name="services">The service collection to which ASP.Net Core extensions should be added.</param>
    /// <param name="builder">Indicates whether authentication is required by default.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddAspNetCoreExtensions(
        this IServiceCollection services,
#if DEBUG
        AspNetCoreExtensionBuilder? builder
#else
        AspNetCoreExtensionBuilder? builder = default
#endif
    )
    {
        builder ??= new();

        services.AddHealthChecks();

        services.TryAddCommonOpenApiExtensions();
        services.TryAddAspNetCoreSearchQuery();

        services.AddAccessor<CultureInfo>();

        services.AddHttpContextAccessor();
        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.TryAddTransient(sp =>
            sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ??
            ClaimsPrincipal.Current ??
            new ClaimsPrincipal(new ClaimsIdentity())
            );

        services.AddSwaggerGen();

        if (builder.RequireAuthenticatedByDefault)
        {
            services.AddRequireAuthenticatedUser(
                builder.RequireApplicationUserId,
                builder.AuthorizationPolicyBuilder
                );
        }

        return services;
    }

    /// <summary>
    /// Adds authentication requirements to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which authentication requirements should be added.</param>
    /// <param name="requireApplicationUserId">Indicates whether the application user ID is required.</param>
    /// <param name="authorizationPolicyBuilder">Action to configure the authorization policy builder.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddRequireAuthenticatedUser(
        this IServiceCollection services,
#if DEBUG
        bool requireApplicationUserId,
        Action<AuthorizationPolicyBuilder>? authorizationPolicyBuilder
#else
        bool requireApplicationUserId = true, //TODO: fix this too
        Action<AuthorizationPolicyBuilder>? authorizationPolicyBuilder = null
#endif
    )
    {
        // Adding the UserAuthorizationHandler that connects B2C Bearer tokens to internal users
        services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();

        // Policy builder
        var policyBuilder = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new UserAuthorizationRequirement(requireApplicationUserId));
        authorizationPolicyBuilder?.Invoke(policyBuilder);

        var authorizationPolicy = policyBuilder.Build();

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(authorizationPolicy);

        services.AddControllers(options => options.Filters.Add(new AuthorizeFilter(authorizationPolicy)));

        return services;
    }

    /// <summary>
    /// Enables extensions for Swagger/OpenAPI (included in AddAspNetCoreExtensions).
    /// </summary>
    /// <param name="services">The service collection to which Swagger/OpenAPI extensions should be added.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddCommonOpenApiExtensions(this IServiceCollection services)
    {
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, AddOperationFilterOptions<FormFileOperationFilter>>();
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, AdditionalSwaggerGenEndpointsOptions>();
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, HealthCheckSwaggerGenEndpointOptions>();
        services.AddSingleton<IConfigureOptions<SwaggerUIOptions>, AdditionalSwaggerUIEndpointsOptions>();
        services.AddControllers(opt => opt.Conventions.Add(new ApiNamespaceControllerModelConvention()));
        return services;
    }

    /// <summary>
    /// Enables extensions for shared Search Query extensions (included in AddAspNetCoreExtensions).
    /// </summary>
    /// <param name="services">The service collection to which Search Query extensions should be added.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddAspNetCoreSearchQuery(
        this IServiceCollection services
        )
    {
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, AddOperationFilterOptions<SearchQueryOperationFilter>>();
        services.AddSingleton<IConfigureOptions<MvcOptions>, AddMvcFilterOptions<SearchQueryResultFilter>>();
        services.AddAccessor<ISearchQuery>();
        services.TryAddSingleton<SearchQueryResultFilter>();
        services.TryAddSingleton<ISearchModelMapper, SearchModelMapper>();
        services.TryAddSingleton<ISearchModelBuilder, SearchModelBuilder>();
        return services;
    }
}
