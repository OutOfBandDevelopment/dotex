using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating.EntityFrameworkCore;

/// <summary>
/// Versioned template storage. A publish adds a row with the next version number and never changes an earlier one, so
/// every version stays available for rollback and audit; two concurrent publishes cannot take the same number because
/// of the unique index, and the loser retries.
/// </summary>
/// <param name="factory">Creates a context per call.</param>
/// <param name="timeProvider">The clock for the publish time.</param>
public sealed class EfTemplateRepository(IDbContextFactory<TemplatesDbContext> factory, TimeProvider timeProvider)
{
    private const int _publishAttempts = 5;

    /// <summary>Publishes a new version of a template.</summary>
    /// <param name="request">The template to publish.</param>
    /// <returns>The stored row with its version number.</returns>
    public async Task<TemplateRow> PublishAsync(TemplatePublish request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name, nameof(request));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ContentType, nameof(request));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CreatedBy, nameof(request));

        for (var attempt = 1; ; attempt++)
        {
            var db = await factory.CreateDbContextAsync().ConfigureAwait(false);
            await using var disposer = db.ConfigureAwait(false);
            var latest = await db.Templates
                .Where(t => t.Name == request.Name && t.Culture == request.Culture && t.TenantId == request.TenantId)
                .MaxAsync(t => (int?)t.Version).ConfigureAwait(false);
            var row = new TemplateRow
            {
                Name = request.Name,
                Version = (latest ?? 0) + 1,
                Culture = request.Culture,
                TenantId = request.TenantId,
                ContentType = request.ContentType,
                TargetContentType = request.TargetContentType,
                TargetFileExtension = request.TargetFileExtension,
                Content = request.Content,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                CreatedAtTicks = timeProvider.GetUtcNow().UtcTicks,
            };
            db.Templates.Add(row);
            try
            {
                await db.SaveChangesAsync().ConfigureAwait(false);
                return row;
            }
            catch (DbUpdateException) when (attempt < _publishAttempts)
            {
                // Another publisher took this version number; read the new maximum and try again.
            }
        }
    }

    /// <summary>
    /// Gets the active template: the tenant's own when it has one, otherwise the shared one. Without a version it is the
    /// highest version.
    /// </summary>
    /// <param name="name">The template name.</param>
    /// <param name="culture">The culture code, or empty.</param>
    /// <param name="tenantId">The tenant, or empty.</param>
    /// <param name="version">A specific version, or <see langword="null"/> for the latest.</param>
    /// <returns>The row, or <see langword="null"/> when none is active.</returns>
    public async Task<TemplateRow?> GetAsync(string name, string culture = "", string tenantId = "", int? version = null)
    {
        var db = await factory.CreateDbContextAsync().ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        var candidates = await db.Templates.AsNoTracking()
            .Where(t => t.Name == name && t.Culture == culture && t.IsActive && (t.TenantId == tenantId || t.TenantId == string.Empty))
            .ToListAsync().ConfigureAwait(false);
        return candidates
            .Where(t => version == null || t.Version == version)
            .OrderByDescending(t => t.TenantId.Length > 0)
            .ThenByDescending(t => t.Version)
            .FirstOrDefault();
    }

    /// <summary>Every version of a template, newest first, including withdrawn ones.</summary>
    /// <param name="name">The template name.</param>
    /// <param name="culture">The culture code, or empty.</param>
    /// <param name="tenantId">The tenant, or empty.</param>
    /// <returns>The version history.</returns>
    public async Task<IReadOnlyList<TemplateRow>> GetHistoryAsync(string name, string culture = "", string tenantId = "")
    {
        var db = await factory.CreateDbContextAsync().ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        return await db.Templates.AsNoTracking()
            .Where(t => t.Name == name && t.Culture == culture && t.TenantId == tenantId)
            .OrderByDescending(t => t.Version)
            .ToListAsync().ConfigureAwait(false);
    }

    /// <summary>Publishes the content of an earlier version as a new version (the history stays linear).</summary>
    /// <param name="name">The template name.</param>
    /// <param name="toVersion">The version to restore.</param>
    /// <param name="restoredBy">Who restores it.</param>
    /// <param name="culture">The culture code, or empty.</param>
    /// <param name="tenantId">The tenant, or empty.</param>
    /// <returns>The new version.</returns>
    public async Task<TemplateRow> RollbackAsync(string name, int toVersion, string restoredBy, string culture = "", string tenantId = "")
    {
        var history = await GetHistoryAsync(name, culture, tenantId).ConfigureAwait(false);
        var source = history.FirstOrDefault(t => t.Version == toVersion)
            ?? throw new InvalidOperationException($"Template '{name}' has no version {toVersion}.");
        return await PublishAsync(new TemplatePublish(
            name,
            source.ContentType,
            source.Content,
            restoredBy,
            culture,
            tenantId,
            source.TargetContentType,
            source.TargetFileExtension,
            $"Restored from version {toVersion}")).ConfigureAwait(false);
    }

    /// <summary>Withdraws a template: every version stays for the audit trail but none is served until a new version is published.</summary>
    /// <param name="name">The template name.</param>
    /// <param name="culture">The culture code, or empty.</param>
    /// <param name="tenantId">The tenant, or empty.</param>
    /// <returns>The number of versions withdrawn.</returns>
    public async Task<int> WithdrawAsync(string name, string culture = "", string tenantId = "")
    {
        var db = await factory.CreateDbContextAsync().ConfigureAwait(false);
        await using var disposer = db.ConfigureAwait(false);
        return await db.Templates
            .Where(t => t.Name == name && t.Culture == culture && t.TenantId == tenantId && t.IsActive)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false)).ConfigureAwait(false);
    }
}
