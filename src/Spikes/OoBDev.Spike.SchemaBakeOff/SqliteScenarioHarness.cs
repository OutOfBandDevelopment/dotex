using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OoBDev.Data.DesiredState;

namespace OoBDev.Spike.SchemaBakeOff;

/// <summary>One throwaway SQLite database folder plus the sqldef runs the bake-off scenarios need.</summary>
internal sealed class SqliteScenarioHarness : IDisposable
{
    private readonly ProcessToolRunner _runner = new();
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "bakeoff_" + global::System.Guid.NewGuid().ToString("N"));

    public SqliteScenarioHarness() => Directory.CreateDirectory(_folder);

    public string DatabasePath => Path.Combine(_folder, "scenario.db");

    public static bool ToolAvailable(string tool)
    {
        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (File.Exists(Path.Combine(dir, tool + ".exe")) || File.Exists(Path.Combine(dir, tool)))
                return true;
        }

        return false;
    }

    public Task<ToolResult> DryRunAsync(string sql, CancellationToken token) => Run(sql, ["--dry-run"], token);

    public Task<ToolResult> ApplyAsync(string sql, bool enableDrop, CancellationToken token) => Run(sql, enableDrop ? ["--enable-drop"] : [], token);

    public Task<ToolResult> CheckAsync(string sql, CancellationToken token) => Run(sql, ["--check"], token);

    public async Task ExecuteAsync(string sql)
    {
        await using var connection = Open();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    public Task<IReadOnlyList<string>> ColumnsAsync(string table) =>
        QueryNamesAsync($"SELECT name FROM pragma_table_info('{table}') ORDER BY cid");

    public Task<IReadOnlyList<string>> ObjectsAsync(string type) =>
        QueryNamesAsync($"SELECT name FROM sqlite_master WHERE type = '{type}' AND name NOT LIKE 'sqlite_%' ORDER BY name");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private async Task<IReadOnlyList<string>> QueryNamesAsync(string sql)
    {
        var names = new List<string>();
        await using var connection = Open();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            names.Add(reader.GetString(0));
        return names;
    }

    private SqliteConnection Open() => new($"Data Source={DatabasePath};Pooling=False");

    private Task<ToolResult> Run(string sql, string[] flags, CancellationToken token) =>
        _runner.RunAsync("sqlite3def", [.. flags, DatabasePath], sql, new Dictionary<string, string>(), token);
}
