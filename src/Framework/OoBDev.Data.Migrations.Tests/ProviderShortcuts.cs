using Microsoft.EntityFrameworkCore;
using OoBDev.Communications.EntityFrameworkCore;
using OoBDev.ComplexEvents.EntityFrameworkCore;
using PostgresMigrations = OoBDev.Data.Migrations.Postgres;
using SqliteMigrations = OoBDev.Data.Migrations.Sqlite;

namespace OoBDev.Data.Migrations.Tests;

internal static class ProviderShortcuts
{
    public static DbContextOptionsBuilder<SuppressionsDbContext> UseSuppressionsSqlite(this DbContextOptionsBuilder<SuppressionsDbContext> b, string cs)
    { SqliteMigrations.DbContextOptionsBuilderExtensions.UseSuppressionsMigrations(b, cs); return b; }
    public static DbContextOptionsBuilder<SuppressionsDbContext> UseSuppressionsPostgres(this DbContextOptionsBuilder<SuppressionsDbContext> b, string cs)
    { PostgresMigrations.DbContextOptionsBuilderExtensions.UseSuppressionsMigrations(b, cs); return b; }
    public static DbContextOptionsBuilder<ScheduleDbContext> UseScheduleSqlite(this DbContextOptionsBuilder<ScheduleDbContext> b, string cs)
    { SqliteMigrations.DbContextOptionsBuilderExtensions.UseScheduleMigrations(b, cs); return b; }
    public static DbContextOptionsBuilder<ScheduleDbContext> UseSchedulePostgres(this DbContextOptionsBuilder<ScheduleDbContext> b, string cs)
    { PostgresMigrations.DbContextOptionsBuilderExtensions.UseScheduleMigrations(b, cs); return b; }
}
