using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Stores read models and projection checkpoints. Selected by the configured key <c>OoBDev:EventSourcing:EntityStore:Type</c>.
/// </summary>
public interface IEntityStore
{
    /// <summary>Gets one entity, or null.</summary>
    Task<EntityRecord?> GetAsync(string type, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an entity if the stored version equals <paramref name="expectedVersion"/> (zero when it must not exist yet).
    /// </summary>
    /// <returns>The saved record, or null on a version conflict.</returns>
    Task<EntityRecord?> SaveAsync(string type, string key, string value, long expectedVersion, CancellationToken cancellationToken = default);

    /// <summary>Deletes an entity; returns false when it did not exist or the version did not match (when given).</summary>
    Task<bool> DeleteAsync(string type, string key, long? expectedVersion = null, CancellationToken cancellationToken = default);

    /// <summary>Lists the entities of a type whose key starts with the prefix, ordered by key.</summary>
    Task<IReadOnlyList<EntityRecord>> QueryAsync(string type, string keyPrefix = "", CancellationToken cancellationToken = default);
}
