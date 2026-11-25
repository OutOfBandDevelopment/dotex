using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.DataLoader.EntityFrameworkCore.Tests;

/// <summary>The EF sink and the loader against the SQL Server container, in a throwaway database.</summary>
[TestClass]
public sealed class EfRowSinkSqlServerTests
{
    public required TestContext TestContext { get; set; }

    [IntegrationTestMethod("oobd-test-sqlserver")]
    [TestCategory(TestCategories.Integration)]
    public async Task Load_CsvWithDuplicate_AppliesThePolicyInSqlServer()
    {
        // Stage
        var master = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING"));
        var name = $"DataLoader_{Guid.NewGuid():N}";
        TestContext.WriteLine("Creating throwaway database {0}", name);
        await ExecuteAsync(master, $"CREATE DATABASE [{name}]");
        try
        {
            var connection = new SqlConnectionStringBuilder(master) { InitialCatalog = name }.ConnectionString;
            var factory = new Factory(connection);
            await using (var db = factory.CreateDbContext())
            {
                await db.Database.EnsureCreatedAsync(TestContext.CancellationToken);
            }

            var services = new ServiceCollection();
            services.TryAddDataLoader();
            services.AddSingleton<IRowSink>(new EfRowSink(factory, "people", "id"));
            await using var provider = services.BuildServiceProvider();
            using var csv = new MemoryStream(Encoding.UTF8.GetBytes("id,name\n1,Ann\n2,Bo\n1,Dup\n"));

            // Test
            var report = await provider.GetRequiredService<ILoader>().LoadAsync(csv, "csv", new LoadOptions(), TestContext.CancellationToken);
            TestContext.WriteLine("read={0} written={1} skipped={2}", report.Read, report.Written, report.Skipped);
            var kept = await new EfRowSink(factory, "people", "id").GetAsync("1", TestContext.CancellationToken);

            // Assert
            Assert.AreEqual(2, report.Written);
            Assert.AreEqual(1, report.Skipped);
            Assert.AreEqual("Ann", kept!["name"]);
        }
        finally
        {
            SqlConnection.ClearAllPools();
            await ExecuteAsync(master, $"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]");
        }
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed class Factory(string connectionString) : IDbContextFactory<LoadedRowsDbContext>
    {
        public LoadedRowsDbContext CreateDbContext()
            => new(new DbContextOptionsBuilder<LoadedRowsDbContext>().UseSqlServer(connectionString).Options);
    }
}
