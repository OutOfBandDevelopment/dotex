using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OoBDev.EventSourcing.EntityFrameworkCore;

namespace OoBDev.EventSourcing.Postgres;

internal sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<EventSourcingDbContext>
{
    public EventSourcingDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<EventSourcingDbContext>()
            .UseNpgsql("Host=localhost;Database=design", b => b.MigrationsAssembly(typeof(PostgresGlobals).Assembly.GetName().Name))
            .Options);
}
