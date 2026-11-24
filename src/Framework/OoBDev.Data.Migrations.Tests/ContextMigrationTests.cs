using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.Communications.EntityFrameworkCore;
using OoBDev.ComplexEvents.EntityFrameworkCore;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;
using System;
using System.Linq;
using System.Threading.Tasks;
using PostgresMigrations = OoBDev.Data.Migrations.Postgres;
using SqliteMigrations = OoBDev.Data.Migrations.Sqlite;

namespace OoBDev.Data.Migrations.Tests;

[TestClass]
public class ContextMigrationTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Suppressions_SnapshotsMatchTheModel()
    {
        using var sqlite = new SuppressionsDbContext(new DbContextOptionsBuilder<SuppressionsDbContext>()
            .UseSuppressionsSqlite("Data Source=unused.db").Options);
        using var postgres = new SuppressionsDbContext(new DbContextOptionsBuilder<SuppressionsDbContext>()
            .UseSuppressionsPostgres("Host=unused;Database=unused").Options);

        Assert.IsFalse(sqlite.Database.HasPendingModelChanges());
        Assert.IsFalse(postgres.Database.HasPendingModelChanges());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Schedule_SnapshotsMatchTheModel()
    {
        using var sqlite = new ScheduleDbContext(new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseScheduleSqlite("Data Source=unused.db").Options);
        using var postgres = new ScheduleDbContext(new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSchedulePostgres("Host=unused;Database=unused").Options);

        Assert.IsFalse(sqlite.Database.HasPendingModelChanges());
        Assert.IsFalse(postgres.Database.HasPendingModelChanges());
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task SqliteMigrate_CreatesSuppressionsAndScheduleSideBySide()
    {
        var connectionString = $"Data Source=ctx_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var keeper = new SqliteConnection(connectionString);
        await keeper.OpenAsync(TestContext.CancellationToken);
        await using var suppressions = new SuppressionsDbContext(new DbContextOptionsBuilder<SuppressionsDbContext>().UseSuppressionsSqlite(connectionString).Options);
        await using var schedule = new ScheduleDbContext(new DbContextOptionsBuilder<ScheduleDbContext>().UseScheduleSqlite(connectionString).Options);

        await suppressions.Database.MigrateAsync(TestContext.CancellationToken);
        await schedule.Database.MigrateAsync(TestContext.CancellationToken);

        Assert.AreEqual(0, await suppressions.Suppressions.CountAsync(TestContext.CancellationToken));
        Assert.AreEqual(0, await schedule.Set<ScheduleRow>().CountAsync(TestContext.CancellationToken));
        await using var command = keeper.CreateCommand();
        command.CommandText = "SELECT count(*) FROM sqlite_master WHERE name IN ('__EFMigrationsHistory_Suppressions','__EFMigrationsHistory_Schedule')";
        Assert.AreEqual(2L, await command.ExecuteScalarAsync(TestContext.CancellationToken));
    }

    [IntegrationTestMethod("oobd-test-postgres")]
    [TestCategory(TestCategories.Integration)]
    public async Task PostgresMigrate_CreatesSuppressionsAndScheduleSideBySide()
    {
        var admin = new NpgsqlConnectionStringBuilder(TestContext.GetRequiredProperty<string>("POSTGRES_CONNECTION_STRING"));
        Assert.IsTrue(admin.Database!.Contains("test", StringComparison.OrdinalIgnoreCase), "Refusing to run against a database that is not a test instance.");
        var name = $"ctx_test_{Guid.NewGuid():N}";
        await using (var connection = new NpgsqlConnection(admin.ConnectionString))
        {
            await connection.OpenAsync(TestContext.CancellationToken);
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await create.ExecuteNonQueryAsync(TestContext.CancellationToken);
        }

        try
        {
            var target = new NpgsqlConnectionStringBuilder(admin.ConnectionString) { Database = name }.ConnectionString;
            await using var suppressions = new SuppressionsDbContext(new DbContextOptionsBuilder<SuppressionsDbContext>().UseSuppressionsPostgres(target).Options);
            await using var schedule = new ScheduleDbContext(new DbContextOptionsBuilder<ScheduleDbContext>().UseSchedulePostgres(target).Options);

            await suppressions.Database.MigrateAsync(TestContext.CancellationToken);
            await schedule.Database.MigrateAsync(TestContext.CancellationToken);

            Assert.AreEqual(0, await suppressions.Suppressions.CountAsync(TestContext.CancellationToken));
            Assert.AreEqual(0, await schedule.Set<ScheduleRow>().CountAsync(TestContext.CancellationToken));
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await using var connection = new NpgsqlConnection(admin.ConnectionString);
            await connection.OpenAsync(TestContext.CancellationToken);
            await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", connection);
            await drop.ExecuteNonQueryAsync(TestContext.CancellationToken);
        }
    }
}
