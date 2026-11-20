using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OoBDev.ComplexEvents.EntityFrameworkCore;

namespace OoBDev.Data.Migrations.Postgres;

internal sealed class ScheduleDesignTimeFactory : IDesignTimeDbContextFactory<ScheduleDbContext>
{
    public ScheduleDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseNpgsql("Host=localhost;Database=design", b => b.MigrationsAssembly(typeof(MigrationsGlobals).Assembly.GetName().Name).MigrationsHistoryTable(MigrationsGlobals.ScheduleHistoryTable))
            .Options);
}
