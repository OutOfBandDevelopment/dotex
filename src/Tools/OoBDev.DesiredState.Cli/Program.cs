using System;
using System.Data.Common;
using System.Threading.Tasks;
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
        _ => throw new NotSupportedException($"deploy supports sqlite and postgres, not '{engine}'."),
    };
}
