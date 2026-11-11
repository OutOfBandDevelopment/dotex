using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OoBDev.EventSourcing.EntityFrameworkCore;

namespace OoBDev.EventSourcing.Sqlite;

internal sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<EventSourcingDbContext>
{
    public EventSourcingDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<EventSourcingDbContext>()
            .UseSqlite("Data Source=design.db", b => b.MigrationsAssembly(typeof(SqliteGlobals).Assembly.GetName().Name))
            .Options);
}
