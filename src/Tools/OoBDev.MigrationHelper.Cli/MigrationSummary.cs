namespace OoBDev.MigrationHelper.Cli;

/// <summary>What a <see cref="PrefixMigrator"/> run changed (or would change in a dry run).</summary>
/// <param name="FoldersRenamed">Folders renamed.</param>
/// <param name="FilesRenamed">Files renamed.</param>
/// <param name="FilesRewritten">Text files whose content was rewritten.</param>
/// <param name="FilesSkipped">Binary files that contain the prefix and were left alone.</param>
public sealed record MigrationSummary(int FoldersRenamed, int FilesRenamed, int FilesRewritten, int FilesSkipped);
