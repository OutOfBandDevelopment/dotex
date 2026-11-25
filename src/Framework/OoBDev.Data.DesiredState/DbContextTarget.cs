using System;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace OoBDev.Data.DesiredState;

/// <summary>Maps an EF Core context to the engine key, the database target and the final-state script.</summary>
internal static class DbContextTarget
{
    private static readonly string[] _hosts = ["Host", "Server", "Data Source", "Address", "Addr", "Network Address"];
    private static readonly string[] _databases = ["Database", "Initial Catalog"];
    private static readonly string[] _users = ["Username", "User Name", "User ID", "UserId", "UID", "User"];
    private static readonly string[] _passwords = ["Password", "PWD"];

    public static string EngineOf(DbContext context)
    {
        var provider = context.Database.ProviderName ?? string.Empty;
        if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return SqldefProvider.Postgres;
        }

        if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            return SqldefProvider.Sqlite;
        }

        throw new NotSupportedException($"Desired state supports PostgreSQL and SQLite contexts; '{provider}' is not supported (SQL Server keeps its dacpac route).");
    }

    public static DatabaseTarget TargetOf(DbContext context, string engine)
    {
        var connectionString = context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"{context.GetType().Name} has no connection string.");
        }

        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        if (engine == SqldefProvider.Sqlite)
        {
            var file = Get(builder, ["Data Source", "DataSource", "Filename"]);
            if (string.IsNullOrWhiteSpace(file) || file.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Get(builder, ["Mode"]), "Memory", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("The tool runs in another process, so the SQLite database must be a file, not an in-memory database.");
            }

            return DatabaseTarget.File(file);
        }

        var host = Get(builder, _hosts) ?? "localhost";
        var port = int.TryParse(Get(builder, ["Port"]), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : 5432;
        var database = Get(builder, _databases) ?? throw new InvalidOperationException("The connection string has no database.");
        return new DatabaseTarget(host, port, database, Get(builder, _users), Get(builder, _passwords));
    }

    public static string ScriptOf(DbContext context)
    {
        var seeded = context.GetService<IDesignTimeModel>().Model.GetEntityTypes().FirstOrDefault(e => e.GetSeedData().Any());
        if (seeded is not null)
        {
            throw new InvalidOperationException($"{seeded.DisplayName()} has seed data (HasData). A final state holds no rows: move the rows to a *.once.sql ensure script.");
        }

        var script = context.Database.GenerateCreateScript();
        return script.Replace("\r\n", "\n", StringComparison.Ordinal).Trim() + "\n";
    }

    private static string? Get(DbConnectionStringBuilder builder, string[] keys)
    {
        foreach (var key in keys)
        {
            if (builder.TryGetValue(key, out var value) && value is not null)
            {
                return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        return null;
    }
}
