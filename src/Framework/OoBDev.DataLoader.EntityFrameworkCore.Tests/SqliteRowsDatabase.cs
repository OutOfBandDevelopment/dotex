using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace OoBDev.DataLoader.EntityFrameworkCore.Tests;

internal sealed class SqliteRowsDatabase : IAsyncDisposable
{
    private readonly SqliteConnection keeper;

    public SqliteRowsDatabase()
    {
        var connectionString = $"Data Source=rows_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        keeper = new SqliteConnection(connectionString);
        keeper.Open();
        Factory = new ContextFactory(connectionString);
        using var db = Factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    public IDbContextFactory<LoadedRowsDbContext> Factory { get; }

    public ValueTask DisposeAsync() => keeper.DisposeAsync();

    private sealed class ContextFactory(string connectionString) : IDbContextFactory<LoadedRowsDbContext>
    {
        public LoadedRowsDbContext CreateDbContext()
            => new(new DbContextOptionsBuilder<LoadedRowsDbContext>().UseSqlite(connectionString).Options);
    }
}
