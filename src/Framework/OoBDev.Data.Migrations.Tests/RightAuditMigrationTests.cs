using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.AspNetCore.EntityFrameworkCore;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;
using System;
using System.Linq;
using System.Threading.Tasks;
using PostgresMigrations = OoBDev.Data.Migrations.Postgres;
using SqliteMigrations = OoBDev.Data.Migrations.Sqlite;

namespace OoBDev.Data.Migrations.Tests;

[TestClass]
public class RightAuditMigrationTests
{
    public TestContext TestContext { get; set; } = null!;

    private static RightAuditDbContext Create(Action<DbContextOptionsBuilder> configure)
    {
        var builder = new DbContextOptionsBuilder<RightAuditDbContext>();
        configure(builder);
        return new RightAuditDbContext(builder.Options);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SqliteMigrations_MatchTheModel()
    {
        using var db = Create(b => SqliteMigrations.DbContextOptionsBuilderExtensions.UseRightAuditMigrations(b, "Data Source=unused.db"));

        Assert.IsFalse(db.Database.HasPendingModelChanges());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void PostgresMigrations_MatchTheModel()
    {
        using var db = Create(b => PostgresMigrations.DbContextOptionsBuilderExtensions.UseRightAuditMigrations(b, "Host=unused;Database=unused"));

        Assert.IsFalse(db.Database.HasPendingModelChanges());
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task SqliteMigrate_CreatesTheTableAndItsOwnHistoryTable()
    {
        var connectionString = $"Data Source=ra_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var keeper = new SqliteConnection(connectionString);
        await keeper.OpenAsync(TestContext.CancellationToken);
        await using var db = Create(b => SqliteMigrations.DbContextOptionsBuilderExtensions.UseRightAuditMigrations(b, connectionString));

        await db.Database.MigrateAsync(TestContext.CancellationToken);
        db.RightAudit.Add(new RightAuditRow { AtTicks = 1, Subject = "grace", Rights = "a", Result = "Allowed" });
        await db.SaveChangesAsync(TestContext.CancellationToken);

        Assert.AreEqual(1, await db.RightAudit.CountAsync(TestContext.CancellationToken));
        var applied = await db.Database.GetAppliedMigrationsAsync(TestContext.CancellationToken);
        Assert.ContainsSingle(applied.ToList());
        await using var command = keeper.CreateCommand();
        command.CommandText = $"SELECT count(*) FROM sqlite_master WHERE name = '{SqliteMigrations.MigrationsGlobals.RightAuditHistoryTable}'";
        Assert.AreEqual(1L, await command.ExecuteScalarAsync(TestContext.CancellationToken));
    }

    [IntegrationTestMethod("oobd-test-postgres")]
    [TestCategory(TestCategories.Integration)]
    public async Task PostgresMigrate_CreatesTheTableOnAnEmptyDatabase()
    {
        var admin = new NpgsqlConnectionStringBuilder(TestContext.GetRequiredProperty<string>("POSTGRES_CONNECTION_STRING"));
        Assert.IsTrue(admin.Database!.Contains("test", StringComparison.OrdinalIgnoreCase), "Refusing to run against a database that is not a test instance.");
        var name = $"ra_test_{Guid.NewGuid():N}";
        await using (var connection = new NpgsqlConnection(admin.ConnectionString))
        {
            await connection.OpenAsync(TestContext.CancellationToken);
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await create.ExecuteNonQueryAsync(TestContext.CancellationToken);
        }

        try
        {
            var target = new NpgsqlConnectionStringBuilder(admin.ConnectionString) { Database = name }.ConnectionString;
            await using var db = Create(b => PostgresMigrations.DbContextOptionsBuilderExtensions.UseRightAuditMigrations(b, target));

            await db.Database.MigrateAsync(TestContext.CancellationToken);
            db.RightAudit.Add(new RightAuditRow { AtTicks = 1, Subject = "grace", Rights = "a", Result = "Allowed" });
            await db.SaveChangesAsync(TestContext.CancellationToken);

            Assert.AreEqual(1, await db.RightAudit.CountAsync(TestContext.CancellationToken));
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
