using OoBDev.Common.ComponentModel;
using OoBDev.Common.Data;
using OoBDev.Common.Net.Http;
using OoBDev.Common.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OoBDev.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOoBDevServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OoBDevClientOptions>(options => configuration.Bind(OoBDevClientOptions.OptionName, options));

        services.TryAddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        services.TryAddTransient(_ => new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            IgnoreReadOnlyProperties = true,
            IgnoreReadOnlyFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
#if DEBUG
            WriteIndented = true,
#endif
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        services.TryAddTransient<IHttpPrepareRequest, HttpPrepareRequest>();
        services.AddTransient<IHttpPrepareRequestFeature, OoBDevHttpPrepareRequestFeature>();

        services.TryAddTransient<IJsonSerializer, WrappedJsonSerializer>();
        services.AddTransient(typeof(IDatabaseQuery<>), typeof(DatabaseQuery<>));
        services.TryAddTransient<IDatabaseMapper, SqlDatabaseMapper>();
        services.TryAddSingleton<IDataConverter, DataConverter>();

        return services;
    }

    /// <summary>
    /// Register accessor type that is scoped to as AsyncLocal
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAccessor<TService>(this IServiceCollection services)
        where TService : class
    {
        services.TryAddSingleton(typeof(IAccessor<>), typeof(Accessor<>));
        return services;
    }
}
