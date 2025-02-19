using System.Collections.Generic;
using System.Threading;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Represents a thread-local store for maintaining state.
/// </summary>
public class StateStore
{
    private readonly static AsyncLocal<Dictionary<string, object?>> _state = new();
    private Dictionary<string, object?> Store => _state.Value ??= [];

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    public object? this[string key]
    {
        get => TryGetValue(key, out var value) ? value : default;
        set
        {
            if (!TryAdd(key, value))
                Store[key] = value;
        }
    }

    /// <summary>
    /// Tries to add the specified key and value to the state store.
    /// </summary>
    /// <param name="key">The key of the value to add.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>True if the key and value were added successfully; otherwise, false.</returns>
    public bool TryAdd(string key, object? value) =>
        Store.TryAdd(key, value);

    /// <summary>
    /// Tries to retrieve the value associated with the specified key from the state store.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
    /// <returns>True if the state store contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(string key, out object? value) =>
        Store.TryGetValue(key, out value);
}
