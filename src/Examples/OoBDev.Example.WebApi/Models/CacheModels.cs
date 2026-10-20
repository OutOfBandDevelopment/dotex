using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>A value to keep in the cache.</summary>
public class CacheStoreModel
{
    /// <summary>Gets or sets the cache key.</summary>
    public required string Key { get; set; }

    /// <summary>Gets or sets the text to keep.</summary>
    public required string Value { get; set; }

    /// <summary>Gets or sets how long the value lives, in seconds.</summary>
    public int Seconds { get; set; } = 30;
}

/// <summary>What the cache returned for a key.</summary>
public class CacheLookupModel
{
    /// <summary>Gets or sets the cache key.</summary>
    public required string Key { get; set; }

    /// <summary>Gets or sets a value indicating whether the key was found.</summary>
    public bool Found { get; set; }

    /// <summary>Gets or sets the cached text when found.</summary>
    public string? Value { get; set; }

    /// <summary>Gets or sets the name of the caching provider type that answered.</summary>
    public required string Provider { get; set; }
}
