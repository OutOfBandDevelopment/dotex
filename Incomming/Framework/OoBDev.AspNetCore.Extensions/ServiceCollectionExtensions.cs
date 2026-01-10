using OoBDev.AspNetCore.Extensions.Middleware;
using OoBDev.AspNetCore.Extensions.SwaggerGen;
using OoBDev.Common;
using OoBDev.Common.Logging;
using OoBDev.Common.Net.Http;
using OoBDev.SemanticKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OoBDev.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOoBDevAspNetCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenApiOptions>(options => configuration.Bind(OpenApiOptions.OptionName, options));

        services.AddAccessor<CorrelationInfo>();

        services.AddHttpClient();

        services.AddTransient<IHttpPrepareRequestFeature, CorrelationInfoHttpPrepareRequestFeature>();

        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, AdditionalSwaggerGenEndpointsOptions>();

        services.AddSwaggerGen(opt => opt.DocumentFilter<OoBDevInternalDocumentFilter>());

        services.Replace(ServiceDescriptor.Scoped<IModelNameAccessor, HttpModelNameAccessor>());
        services.Replace(ServiceDescriptor.Scoped<ICurrentUserAccessor, HttpCurrentUserAccessor>());

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.TryAddSingleton<IEnvironmentSettings, EnvironmentSettings>();

        return services;
    }
}
