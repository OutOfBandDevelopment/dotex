using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.Orchestration;

/// <summary>Holds preferences in memory for tests, examples and small hosts; a real host registers its own <see cref="ITargetPreferenceProvider"/> over its user store.</summary>
public sealed class InMemoryTargetPreferenceProvider : ITargetPreferenceProvider
{
    private readonly ConcurrentDictionary<string, TargetPreference> _preferences = new(StringComparer.Ordinal);

    /// <summary>Adds or replaces the preference of <see cref="TargetPreference.TargetId"/>.</summary>
    public void Set(TargetPreference preference)
    {
        ArgumentNullException.ThrowIfNull(preference);
        _preferences[preference.TargetId] = preference;
    }

    /// <inheritdoc />
    public Task<TargetPreference?> GetAsync(string targetId, CancellationToken cancellationToken = default)
        => Task.FromResult(_preferences.TryGetValue(targetId, out var preference) ? preference : null);
}
