using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OoBDev.Communications.EntityFrameworkCore;

namespace OoBDev.Data.Migrations.Sqlite;

internal sealed class SuppressionsDesignTimeFactory : IDesignTimeDbContextFactory<SuppressionsDbContext>
{
    public SuppressionsDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<SuppressionsDbContext>()
            .UseSqlite("Data Source=design.db", b => b.MigrationsAssembly(typeof(MigrationsGlobals).Assembly.GetName().Name).MigrationsHistoryTable(MigrationsGlobals.SuppressionsHistoryTable))
            .Options);
}
