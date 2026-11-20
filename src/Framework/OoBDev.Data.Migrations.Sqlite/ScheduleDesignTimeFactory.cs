using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OoBDev.ComplexEvents.EntityFrameworkCore;

namespace OoBDev.Data.Migrations.Sqlite;

internal sealed class ScheduleDesignTimeFactory : IDesignTimeDbContextFactory<ScheduleDbContext>
{
    public ScheduleDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSqlite("Data Source=design.db", b => b.MigrationsAssembly(typeof(MigrationsGlobals).Assembly.GetName().Name).MigrationsHistoryTable(MigrationsGlobals.ScheduleHistoryTable))
            .Options);
}
