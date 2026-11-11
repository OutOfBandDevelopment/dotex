using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OoBDev.Example.GraphQueries.Tests;

/// <summary>
/// Runs the graph query samples of <c>OoBDev.Example.GraphQueries.Tests/Graph</c> against SQL Server (node and edge tables), PostgreSQL and SQLite (recursive CTEs).
/// The graph: Ann-Bob-Cat-Dan, Ann-Eve-Dan (undirected), Fay alone.
/// </summary>
[TestClass]
public class GraphQueryTests
{
    private static readonly string[] FriendsOfAnn = ["Bob", "Cat", "Dan", "Eve"];

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Scripts_AreEmbedded()
    {
        var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        Assert.IsTrue(names.Any(n => n.EndsWith("Recursive.ShortestPath.sql", StringComparison.Ordinal)));
        Assert.IsTrue(names.Any(n => n.EndsWith("SqlServer.ShortestPath.sql", StringComparison.Ordinal)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Sqlite_RecursiveCte_AnswersTheQuestions()
    {
        // Stage
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        // Test
        await RunBatchesAsync(connection, "Recursive.Schema.sql");

        // Assert
        await AssertRecursiveAnswersAsync(connection);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Postgres_RecursiveCte_AnswersTheQuestions()
    {
        // Stage
        var connectionString = TestContext.GetRequiredProperty<string>("POSTGRES_CONNECTION_STRING");
        var schema = $"graph_{Guid.NewGuid():N}";
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await ExecuteAsync(connection, $"CREATE SCHEMA {schema}; SET search_path TO {schema};");
        try
        {
            // Test
            await RunBatchesAsync(connection, "Recursive.Schema.sql");

            // Assert
            await AssertRecursiveAnswersAsync(connection);
        }
        finally
        {
            await ExecuteAsync(connection, $"DROP SCHEMA {schema} CASCADE");
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SqlServer_NodeAndEdgeTables_AnswerTheQuestions()
    {
        // Stage
        var connectionString = TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING");
        var database = $"Test_{Guid.NewGuid():N}";
        await using var master = new SqlConnection(connectionString);
        await master.OpenAsync();
        await ExecuteAsync(master, $"CREATE DATABASE [{database}]");
        try
        {
            await using var connection = new SqlConnection(new SqlConnectionStringBuilder(connectionString) { InitialCatalog = database }.ConnectionString);
            await connection.OpenAsync();

            // Test
            await RunBatchesAsync(connection, "SqlServer.Schema.sql");
            var friends = await QueryAsync(connection, await ReadAsync("SqlServer.FriendsWithinTwoSteps.sql"));
            var path = await QueryAsync(connection, await ReadAsync("SqlServer.ShortestPath.sql"));

            // Assert
            CollectionAssert.AreEqual(FriendsOfAnn, friends.Select(r => (string)r[0]).ToArray());
            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(2, Convert.ToInt32(path[0][1]));
            Assert.AreEqual("Eve>Dan", (string)path[0][0]);
        }
        finally
        {
            await ExecuteAsync(master, $"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]");
        }
    }

    private static async Task AssertRecursiveAnswersAsync(DbConnection connection)
    {
        var friends = await QueryAsync(connection, await ReadAsync("Recursive.FriendsWithinTwoSteps.sql"));
        var path = await QueryAsync(connection, await ReadAsync("Recursive.ShortestPath.sql"));

        CollectionAssert.AreEqual(FriendsOfAnn, friends.Select(r => (string)r[0]).ToArray());
        Assert.AreEqual(1, path.Count);
        Assert.AreEqual(2, Convert.ToInt32(path[0][1]));
        Assert.AreEqual(",1,5,4,", (string)path[0][0]);
    }

    private static async Task<string> ReadAsync(string script)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith(script, StringComparison.Ordinal));
        await using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static async Task RunBatchesAsync(DbConnection connection, string script)
    {
        var text = await ReadAsync(script);
        var batches = Regex.Split(text, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        foreach (var batch in batches.Where(b => !string.IsNullOrWhiteSpace(b)))
            if (connection is SqlConnection)
                await ExecuteAsync(connection, batch);
            else
                foreach (var statement in batch.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Where(s => !s.StartsWith("--", StringComparison.Ordinal) || s.Contains('\n', StringComparison.Ordinal)))
                    await ExecuteAsync(connection, statement);
    }

    private static async Task ExecuteAsync(DbConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<List<object[]>> QueryAsync(DbConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<object[]>();
        while (await reader.ReadAsync())
        {
            var row = new object[reader.FieldCount];
            reader.GetValues(row);
            rows.Add(row);
        }
        return rows;
    }
}
