using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace OoBDev.ComplexEvents.EntityFrameworkCore.Tests;

internal sealed class SqliteScheduleDatabase : IAsyncDisposable
{
    private readonly SqliteConnection keeper;

    public SqliteScheduleDatabase()
    {
        var connectionString = $"Data Source=sched_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        keeper = new SqliteConnection(connectionString);
        keeper.Open();
        Factory = new ContextFactory(connectionString);
        using var db = Factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    public IDbContextFactory<ScheduleDbContext> Factory { get; }

    public ValueTask DisposeAsync() => keeper.DisposeAsync();

    private sealed class ContextFactory(string connectionString) : IDbContextFactory<ScheduleDbContext>
    {
        public ScheduleDbContext CreateDbContext()
            => new(new DbContextOptionsBuilder<ScheduleDbContext>().UseSqlite(connectionString).Options);
    }
}
