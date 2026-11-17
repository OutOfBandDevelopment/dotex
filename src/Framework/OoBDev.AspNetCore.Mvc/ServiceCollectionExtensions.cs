using Microsoft.AspNetCore.Authentication;
using OoBDev.AspNetCore.Mvc.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Authorization;
using OoBDev.AspNetCore.Mvc.Authorization;
using OoBDev.System;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.AspNetCore.Mvc.Middleware;
using OoBDev.AspNetCore.Mvc.Providers.SearchQuery;
using OoBDev.AspNetCore.Mvc.OpenApi;
using OoBDev.Extensions;
using OoBDev.System.Linq.Search;
using OoBDev.System.Net.Http;
using OoBDev.System.Security;
using System;
using System.Linq;
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
        services.TryAddSingleton<HealthCheckPath>();

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

        services.TryAddKeyedTransient<ICurrentUserAccessor, HttpContextUserAccessor>("http");
        services.Replace(ServiceDescriptor.Describe(
            typeof(ICurrentUserAccessor),
            sp => sp.GetRequiredKeyedService<ICurrentUserAccessor>("http"),
            ServiceLifetime.Transient));

        services.AddTransient<IHttpPrepareRequestFeature, CorrelationInfoHttpPrepareRequestFeature>();

        services.TryAddTransient<ICurrentUserAccessor, EnvironmentUserAccessor>();
        services.TryAddKeyedTransient<ICurrentUserAccessor, EnvironmentUserAccessor>("environment");

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
    /// Adds the options of <see cref="Middleware.CultureInfoMiddleware"/> (<c>OoBDev:Culture</c>: <c>DefaultCulture</c>, <c>SupportedCultures</c>).
    /// Without them the middleware accepts any valid culture and keeps the process culture for a request that names none.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration holding the <c>OoBDev:Culture</c> section.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddCultureNegotiation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<Middleware.CultureInfoMiddlewareOptions>(configuration, "OoBDev:Culture");
        return services;
    }

    /// <summary>
    /// Adds role to application right mapping: validated options (<c>OoBDev:Authorization</c>), the configuration mapper,
    /// the claims transformation and the shared right checker.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration holding the <c>OoBDev:Authorization</c> section.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddRoleRightMapping(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<RoleRightOptions>(configuration, "OoBDev:Authorization");
        services.TryAddSingleton<IRoleRightMapper, ConfigurationRoleRightMapper>();
        services.TryAddSingleton<IApplicationRightChecker, ApplicationRightChecker>();
        services.TryAddTransient<IClaimsTransformation, RoleRightClaimsTransformation>();
        return services;
    }

    /// <summary>
    /// Writes one log entry per application right check (subject, required rights, outcome) by wrapping the registered
    /// <see cref="IApplicationRightChecker"/>. Call it after the checker is registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddRightAuditLog(this IServiceCollection services)
    {
        var existing = services.LastOrDefault(d => d.ServiceType == typeof(IApplicationRightChecker))
            ?? throw new InvalidOperationException("Register an IApplicationRightChecker (for example with TryAddRoleRightMapping) before AddRightAuditLog.");
        services.Remove(existing);
        services.Add(ServiceDescriptor.Describe(typeof(AuditedInner), sp => new AuditedInner(CreateInner(sp, existing)), existing.Lifetime));
        services.Add(ServiceDescriptor.Describe(
            typeof(IApplicationRightChecker),
            sp => new AuditingApplicationRightChecker(sp.GetRequiredService<AuditedInner>().Checker, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AuditingApplicationRightChecker>>(), sp.GetService<IRightAuditSink>(), sp.GetService<TimeProvider>()),
            existing.Lifetime));
        return services;
    }

    private sealed record AuditedInner(IApplicationRightChecker Checker);

    private static IApplicationRightChecker CreateInner(IServiceProvider sp, ServiceDescriptor descriptor) =>
        descriptor.ImplementationInstance as IApplicationRightChecker
        ?? (descriptor.ImplementationFactory?.Invoke(sp) as IApplicationRightChecker)
        ?? (IApplicationRightChecker)ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!);

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
        bool requireApplicationUserId = true, // Request: docs/features/request/2026-11-17-configurable-application-user-claim.md
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
