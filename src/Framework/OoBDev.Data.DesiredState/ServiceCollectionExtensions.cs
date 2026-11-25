using System.Net.Http;
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
        services.TryAddSingleton<EfDesiredStateDeployer>();
        services.TryAddSingleton<SchemaPackageDeployer>();
        return services;
    }

    /// <summary>
    /// Fetches a missing sqldef tool (pinned release, SHA-256 checked, cached per user) instead of requiring it on <c>PATH</c>. Opt-in because it uses the network on first use.
    /// Call it after <see cref="AddDesiredStateDatabase"/>.
    /// </summary>
    public static IServiceCollection AddSqldefDownload(this IServiceCollection services)
    {
        services.AddSingleton<SqldefInstaller>(_ => new SqldefInstaller(new HttpClient()));
        services.AddSingleton<IToolRunner>(sp => new SqldefDownloadingToolRunner(new ProcessToolRunner(), sp.GetRequiredService<SqldefInstaller>()));
        return services;
    }
}
