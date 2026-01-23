using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.TestUtilities.Configuration;

/// <summary>
/// Provides an in-memory configuration implementation for testing.
/// Stores configuration key-value pairs using tuples.
/// </summary>
public class TupleConfiguration : IConfiguration, IConfigurationSection
{
    private readonly IDictionary<string, string> _store = new Dictionary<string, string>();

    /// <summary>
    /// Gets the configuration key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the configuration path (colon-separated hierarchy).
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets or sets the configuration value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the TupleConfiguration class with an array of settings.
    /// </summary>
    /// <param name="settings">Array of key-value tuples.</param>
    public TupleConfiguration(params (string key, string value)[] settings)
        : this(settings.AsEnumerable())
    {
    }

    /// <summary>
    /// Initializes a new instance of the TupleConfiguration class with an enumerable of settings.
    /// </summary>
    /// <param name="settings">Enumerable of key-value tuples.</param>
    /// <param name="key">Optional configuration key.</param>
    /// <param name="path">Optional configuration path.</param>
    public TupleConfiguration(IEnumerable<(string key, string value)> settings, string? key = null, string? path = null)
    {
        if (settings.Count() == 1)
        {
            var first = settings.FirstOrDefault();
            Value = first.value;
            Key = first.key;
            Path = string.Join(":", new[] { path, key }.Where(i => !string.IsNullOrWhiteSpace(i)));
        }
        else
        {
            Key = key ?? "unknown";
            Path = string.Join(":", new[] { path, key }.Where(i => !string.IsNullOrWhiteSpace(i)));
        }
        foreach (var setting in settings)
        {
            this[setting.key] = setting.value;
        }
    }

    /// <summary>
    /// Gets or sets the configuration value for the specified key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value, or null if not found.</returns>
    public string? this[string key]
    {
        get => _store.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (_store.ContainsKey(key))
            {
                if (value == null)
                    _store.Remove(key);
                else
                    _store[key] = value;
            }
            else if (value != null)
            {
                _store.Add(key, value);
            }
        }
    }

    /// <summary>
    /// Returns a change token that is always inactive (no reload support for in-memory configuration).
    /// </summary>
    /// <returns>A change token that never signals changes.</returns>
    public IChangeToken GetReloadToken() => new ChangeToken();

    /// <summary>
    /// Gets the immediate child configuration sections.
    /// </summary>
    /// <returns>Enumerable of child configuration sections.</returns>
    public IEnumerable<IConfigurationSection> GetChildren()
    {
        var values = from k in _store.Keys
                     let p = k.Split([':'], 2)
                     group new
                     {
                         key = p.ElementAtOrDefault(1),
                         value = _store[k],
                     } by p.ElementAtOrDefault(0);

        foreach (var value in values)
            yield return new TupleConfiguration(
                settings: value.Select(i => (i.key, i.value)),
                key: value.Key,
                path: Path
                );
    }

    /// <summary>
    /// Gets a configuration sub-section with the specified key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration section.</returns>
    public IConfigurationSection GetSection(string key) =>
         GetChildren()?.FirstOrDefault(i => i.Key == key) ?? new TupleConfiguration();

    internal class ChangeToken : IChangeToken, IDisposable
    {
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public void Dispose() { }
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => this;
    }

}
