using System;

namespace OoBDev.Search.Requests;

/// <summary>
/// A request names a filter the provider does not support. The message names the filter and never carries its value.
/// </summary>
public sealed class UnsupportedFilterException(string filter) : Exception($"The search provider does not support the filter '{filter}'.")
{
    /// <summary>The filter name.</summary>
    public string Filter { get; } = filter;
}
