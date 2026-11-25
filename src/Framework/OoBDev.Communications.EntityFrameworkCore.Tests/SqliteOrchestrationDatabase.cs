using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.EntityFrameworkCore.Tests;

internal sealed class SqliteOrchestrationDatabase : IAsyncDisposable
{
    private readonly SqliteConnection keeper;

    public SqliteOrchestrationDatabase()
    {
        var connectionString = $"Data Source=orc_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        keeper = new SqliteConnection(connectionString);
        keeper.Open();
        Factory = new ContextFactory(connectionString);
        using var db = Factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    public IDbContextFactory<OrchestrationDbContext> Factory { get; }

    public ValueTask DisposeAsync() => keeper.DisposeAsync();

    private sealed class ContextFactory(string connectionString) : IDbContextFactory<OrchestrationDbContext>
    {
        public OrchestrationDbContext CreateDbContext()
            => new(new DbContextOptionsBuilder<OrchestrationDbContext>().UseSqlite(connectionString).Options);
    }
}
