using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

internal sealed class SqliteDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _keeper;
    private readonly string _connectionString = $"Data Source=es_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

    public SqliteDatabase()
    {
        _keeper = new SqliteConnection(_connectionString);
        _keeper.Open();
        Factory = new ContextFactory(_connectionString);
        using var db = Factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    public IDbContextFactory<EventSourcingDbContext> Factory { get; }

    public ValueTask DisposeAsync() => _keeper.DisposeAsync();

    private sealed class ContextFactory(string connectionString) : IDbContextFactory<EventSourcingDbContext>
    {
        public EventSourcingDbContext CreateDbContext()
            => new(new DbContextOptionsBuilder<EventSourcingDbContext>().UseSqlite(connectionString).Options);
    }
}
