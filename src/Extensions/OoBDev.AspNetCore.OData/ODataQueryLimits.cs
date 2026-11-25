using System.ComponentModel.DataAnnotations;

namespace OoBDev.AspNetCore.OData;

/// <summary>
/// Cost limits for OData system query options.
/// </summary>
public class ODataQueryLimits
{
    /// <summary>Rows returned when the request has no <c>$top</c>.</summary>
    [Range(1, 100_000)]
    public int DefaultPageSize { get; set; } = 50;

    /// <summary>The largest accepted <c>$top</c>.</summary>
    [Range(1, 100_000)]
    public int MaxTop { get; set; } = 100;

    /// <summary>The largest accepted <c>$filter</c> expression, in nodes.</summary>
    [Range(1, 10_000)]
    public int MaxNodeCount { get; set; } = 100;

    /// <summary>Whether <c>$expand</c> is accepted.</summary>
    public bool AllowExpand { get; set; }

    /// <summary>The deepest accepted <c>$expand</c> when it is allowed.</summary>
    [Range(1, 10)]
    public int MaxExpansionDepth { get; set; } = 2;
}
