using Microsoft.EntityFrameworkCore;

namespace OoBDev.Data.Migrations.Postgres;

/// <summary>Points a context at the migration sets of this assembly.</summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the right audit context for Postgres with its migrations assembly and its own history table.
    /// </summary>
    /// <param name="options">The options builder of <c>RightAuditDbContext</c>.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The options builder.</returns>
    public static DbContextOptionsBuilder UseRightAuditMigrations(this DbContextOptionsBuilder options, string connectionString)
        => options.UseNpgsql(connectionString, b => b
            .MigrationsAssembly(typeof(MigrationsGlobals).Assembly.GetName().Name)
            .MigrationsHistoryTable(MigrationsGlobals.RightAuditHistoryTable));
}
