using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OoBDev.Communications.EntityFrameworkCore;

namespace OoBDev.Data.Migrations.Postgres;

internal sealed class SuppressionsDesignTimeFactory : IDesignTimeDbContextFactory<SuppressionsDbContext>
{
    public SuppressionsDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<SuppressionsDbContext>()
            .UseNpgsql("Host=localhost;Database=design", b => b.MigrationsAssembly(typeof(MigrationsGlobals).Assembly.GetName().Name).MigrationsHistoryTable(MigrationsGlobals.SuppressionsHistoryTable))
            .Options);
}
