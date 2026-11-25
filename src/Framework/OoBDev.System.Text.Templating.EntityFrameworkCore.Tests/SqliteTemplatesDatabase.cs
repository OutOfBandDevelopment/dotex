using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating.EntityFrameworkCore.Tests;

internal sealed class SqliteTemplatesDatabase : IAsyncDisposable
{
    private readonly SqliteConnection keeper;

    public SqliteTemplatesDatabase()
    {
        var connectionString = $"Data Source=tpl_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        keeper = new SqliteConnection(connectionString);
        keeper.Open();
        Factory = new ContextFactory(connectionString);
        using var db = Factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    public IDbContextFactory<TemplatesDbContext> Factory { get; }

    public ValueTask DisposeAsync() => keeper.DisposeAsync();

    private sealed class ContextFactory(string connectionString) : IDbContextFactory<TemplatesDbContext>
    {
        public TemplatesDbContext CreateDbContext()
            => new(new DbContextOptionsBuilder<TemplatesDbContext>().UseSqlite(connectionString).Options);
    }
}
