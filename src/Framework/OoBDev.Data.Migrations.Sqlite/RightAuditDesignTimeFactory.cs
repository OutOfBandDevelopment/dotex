using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OoBDev.AspNetCore.EntityFrameworkCore;

namespace OoBDev.Data.Migrations.Sqlite;

internal sealed class RightAuditDesignTimeFactory : IDesignTimeDbContextFactory<RightAuditDbContext>
{
    public RightAuditDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<RightAuditDbContext>()
            .UseSqlite("Data Source=design.db", b => b.MigrationsAssembly(typeof(MigrationsGlobals).Assembly.GetName().Name).MigrationsHistoryTable(MigrationsGlobals.RightAuditHistoryTable))
            .Options);
}
