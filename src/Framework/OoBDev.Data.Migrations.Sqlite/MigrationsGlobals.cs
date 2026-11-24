namespace OoBDev.Data.Migrations.Sqlite;

/// <summary>Names shared by the migration sets in this assembly.</summary>
public static class MigrationsGlobals
{
    /// <summary>The history table of the right audit store.</summary>
    public const string RightAuditHistoryTable = "__EFMigrationsHistory_RightAudit";

    /// <summary>The history table of the suppression store.</summary>
    public const string SuppressionsHistoryTable = "__EFMigrationsHistory_Suppressions";

    /// <summary>The history table of the complex event schedule store.</summary>
    public const string ScheduleHistoryTable = "__EFMigrationsHistory_Schedule";
}
