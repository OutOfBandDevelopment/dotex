namespace OoBDev.Data.DesiredState;

/// <summary>
/// One data-preserving change a schema diff cannot infer (a rename). It runs once per database, before the schema is planned, and is recorded by <paramref name="Id"/>.
/// On a database where <paramref name="Table"/> does not exist yet there is nothing to preserve, so the entry is recorded without running.
/// </summary>
/// <param name="Id">Unique and stable, for example <c>2026-11-24-notes-text-to-body</c>.</param>
/// <param name="Table">The table the change applies to (unquoted, without schema).</param>
/// <param name="Sql">The statement, in the dialect of the target engine (for example <c>ALTER TABLE "Notes" RENAME COLUMN "Text" TO "Body"</c>).</param>
public sealed record RefactorEntry(string Id, string Table, string Sql);
