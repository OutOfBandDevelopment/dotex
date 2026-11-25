using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Data.DesiredState;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers the tool runner, the sqldef providers for PostgreSQL, SQL Server and SQLite (keyed by engine), and the factory.</summary>
    public static IServiceCollection AddDesiredStateDatabase(this IServiceCollection services)
    {
        services.TryAddSingleton<IToolRunner, ProcessToolRunner>();
        services.TryAddKeyedSingleton<IDesiredStateProvider>(SqldefProvider.Postgres, (sp, _) => SqldefProvider.ForPostgres(sp.GetRequiredService<IToolRunner>()));
        services.TryAddKeyedSingleton<IDesiredStateProvider>(SqldefProvider.SqlServer, (sp, _) => SqldefProvider.ForSqlServer(sp.GetRequiredService<IToolRunner>()));
        services.TryAddKeyedSingleton<IDesiredStateProvider>(SqldefProvider.Sqlite, (sp, _) => SqldefProvider.ForSqlite(sp.GetRequiredService<IToolRunner>()));
        services.TryAddSingleton<DesiredStateProviderFactory>();
        services.TryAddSingleton<IAppliedScriptStore, InMemoryAppliedScriptStore>();
        services.TryAddSingleton<EnsureScriptRunner>();
        return services;
    }

}
