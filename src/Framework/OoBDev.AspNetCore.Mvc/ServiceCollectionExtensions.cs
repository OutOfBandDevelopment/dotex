using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Mvc.Authorization;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.AspNetCore.Mvc.Middleware;
using OoBDev.AspNetCore.Mvc.Providers.SearchQuery;
using OoBDev.AspNetCore.Mvc.OpenApi;
using OoBDev.Extensions;
using OoBDev.System.Linq.Search;
using OoBDev.System.Net.Http;
using OoBDev.System.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Reflection;
using System.Security.Principal;

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

        services.TryAddTransient<IIdentity>(sp=>sp.GetRequiredService<IPrincipal>().Identity ?? new ClaimsIdentity());
        services.TryAddTransient<IPrincipal>(sp =>
            sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ??
            ClaimsPrincipal.Current ??
            new ClaimsPrincipal(new ClaimsIdentity())
        );
        services.TryAddTransient(sp =>
            sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ??
            ClaimsPrincipal.Current ??
            new ClaimsPrincipal(new ClaimsIdentity())
            );

        services.TryAddKeyedTransient<ICurrentUserAccessor, HttpContextUserAccessor>("HTTP");
        services.Replace(ServiceDescriptor.Describe(
            typeof(ICurrentUserAccessor),
            sp => sp.GetRequiredKeyedService<ICurrentUserAccessor>("HTTP"),
            ServiceLifetime.Transient));

        services.AddTransient<IHttpPrepareRequestFeature, CorrelationInfoHttpPrepareRequestFeature>();

        services.TryAddTransient<ICurrentUserAccessor, EnvironmentUserAccessor>();
        services.TryAddKeyedTransient<ICurrentUserAccessor, EnvironmentUserAccessor>("Environment");

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
        // Adding the UserAuthorizationHandler that connects Bearer tokens to internal users
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
    /// Enables extensions for OpenAPI (included in AddAspNetCoreExtensions): one document named <c>all</c> and one per assembly
    /// that contains controllers, with application info, permissions, health checks, XML documentation and search query descriptions.
    /// </summary>
    /// <param name="services">The service collection to which OpenAPI extensions should be added.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddCommonOpenApiExtensions(this IServiceCollection services) =>
        services.TryAddCommonOpenApiExtensions(OpenApiDocumentCatalog.DiscoverControllerAssemblies());

    /// <summary>
    /// Enables extensions for OpenAPI (included in AddAspNetCoreExtensions) for specific controller assemblies.
    /// </summary>
    /// <param name="services">The service collection to which OpenAPI extensions should be added.</param>
    /// <param name="controllerAssemblies">The assemblies that contain controllers; each gets its own document beside <c>all</c>.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddCommonOpenApiExtensions(
        this IServiceCollection services,
        IEnumerable<Assembly> controllerAssemblies)
    {
        var catalog = OpenApiDocumentCatalog.From(controllerAssemblies);
        services.TryAddSingleton(catalog);
        services.TryAddSingleton<XmlDocumentationProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenApiOptions>, ConfigureOpenApiOptions>());
        foreach (var name in catalog.Names)
        {
            services.AddOpenApi(name);
        }

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
        services.AddSingleton<IConfigureOptions<MvcOptions>, AddMvcFilterOptions<SearchQueryResultFilter>>();
        services.AddAccessor<ISearchQuery>();
        services.TryAddSingleton<SearchQueryResultFilter>();
        services.TryAddSingleton<ISearchModelMapper, SearchModelMapper>();
        services.TryAddSingleton<ISearchModelBuilder, SearchModelBuilder>();
        return services;
    }
}
