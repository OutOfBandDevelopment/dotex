using OoBDev.Extensions;
using OoBDev.MongoDB.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.MongoDB;

/// <summary>
/// Common libraries to enable MongoDB Support
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enable common infrastructure.  
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="mongoDatabaseConfigurationSection"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddMongoServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string mongoDatabaseConfigurationSection
#else
        string mongoDatabaseConfigurationSection = nameof(MongoDatabaseOptions)
#endif
        )
    {
        services.Configure<MongoDatabaseOptions>(options => configuration.Bind(mongoDatabaseConfigurationSection, options));
        services.TryAddSingleton<IMongoDatabaseFactory, MongoDatabaseFactory>();
        return services;
    }

    /// <summary>
    /// register MongoDatabase instance with custom configuration options
    /// </summary>
    /// <typeparam name="TDatabase"></typeparam>
    /// <typeparam name="TSettings"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddMongoDatabase<TDatabase, TSettings>(this IServiceCollection services)
        where TDatabase : class
        where TSettings : class, IMongoSettings
    {
        services.GetSingletonInstance<IMongoDatabaseRegistration, MongoDatabaseRegistration>(out var db);

        services.TryAddTransient(sp => sp.GetRequiredService<IMongoDatabaseFactory>().Create<TDatabase, TSettings>());
        db.InternalTypes.Add(typeof(TDatabase));
        return services;
    }

    /// <summary>
    /// register MongoDatabase instance that will use the <seealso cref="MongoDatabaseOptions"/> configuration options
    /// </summary>
    /// <typeparam name="TDatabase"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddMongoDatabase<TDatabase>(this IServiceCollection services)
        where TDatabase : class => services.TryAddMongoDatabase<TDatabase, MongoDatabaseOptions>();
}
