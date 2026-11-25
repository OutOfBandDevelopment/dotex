using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OoBDev.Data.DesiredState;

namespace OoBDev.DesiredState.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection().AddDesiredStateDatabase();
        using var provider = services.BuildServiceProvider();
        return await DesiredStateCommand.RunAsync(args, provider.GetRequiredService<DesiredStateProviderFactory>(), Environment.GetEnvironmentVariable, Console.Out, Console.Error, connect: Connect).ConfigureAwait(false);
    }

    private static DbConnection Connect(string engine, DatabaseTarget target) => engine.ToLowerInvariant() switch
    {
        SqldefProvider.Sqlite => new SqliteConnection($"Data Source={target.Database};Pooling=False"),
        SqldefProvider.Postgres => new NpgsqlConnection(new NpgsqlConnectionStringBuilder { Host = target.Host, Port = target.Port > 0 ? target.Port : 5432, Database = target.Database, Username = target.User, Password = target.Password, Pooling = false }.ConnectionString),
        SqldefProvider.SqlServer => new SqlConnection(new SqlConnectionStringBuilder { DataSource = target.Port > 0 ? $"{target.Host},{target.Port}" : target.Host, InitialCatalog = target.Database, UserID = target.User ?? string.Empty, Password = target.Password ?? string.Empty, TrustServerCertificate = true }.ConnectionString),
        _ => throw new NotSupportedException($"deploy supports sqlite, postgres and sqlserver, not '{engine}'."),
    };
}
