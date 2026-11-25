using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.EntityFrameworkCore.Tests;

internal sealed class SqliteSuppressionsDatabase : IAsyncDisposable
{
    private readonly SqliteConnection keeper;

    public SqliteSuppressionsDatabase()
    {
        var connectionString = $"Data Source=sup_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        keeper = new SqliteConnection(connectionString);
        keeper.Open();
        Factory = new ContextFactory(connectionString);
        using var db = Factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    public IDbContextFactory<SuppressionsDbContext> Factory { get; }

    public ValueTask DisposeAsync() => keeper.DisposeAsync();

    private sealed class ContextFactory(string connectionString) : IDbContextFactory<SuppressionsDbContext>
    {
        public SuppressionsDbContext CreateDbContext()
            => new(new DbContextOptionsBuilder<SuppressionsDbContext>().UseSqlite(connectionString).Options);
    }
}
