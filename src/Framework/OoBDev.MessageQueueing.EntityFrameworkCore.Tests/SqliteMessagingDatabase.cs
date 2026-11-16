using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.EntityFrameworkCore.Tests;

internal sealed class SqliteMessagingDatabase : IAsyncDisposable
{
    private readonly SqliteConnection keeper;

    public SqliteMessagingDatabase()
    {
        var connectionString = $"Data Source=mq_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        keeper = new SqliteConnection(connectionString);
        keeper.Open();
        Factory = new ContextFactory(connectionString);
        using var db = Factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    public IDbContextFactory<MessagingDbContext> Factory { get; }

    public ValueTask DisposeAsync() => keeper.DisposeAsync();

    private sealed class ContextFactory(string connectionString) : IDbContextFactory<MessagingDbContext>
    {
        public MessagingDbContext CreateDbContext()
            => new(new DbContextOptionsBuilder<MessagingDbContext>().UseSqlite(connectionString).Options);
    }
}
