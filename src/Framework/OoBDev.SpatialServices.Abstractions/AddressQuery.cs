using System.Linq;

namespace OoBDev.SpatialServices;

/// <summary>An address to look up.</summary>
/// <param name="Line1">Street line.</param>
/// <param name="Line2">Unit or second line.</param>
/// <param name="City">City.</param>
/// <param name="Region">State or province.</param>
/// <param name="PostalCode">Postal code.</param>
/// <param name="Country">Country.</param>
public sealed record AddressQuery(string Line1, string? Line2 = null, string? City = null, string? Region = null, string? PostalCode = null, string? Country = null)
{
    /// <summary>The non-empty parts joined with commas.</summary>
    public string ToOneLine() => string.Join(", ", new[] { Line1, Line2, City, Region, PostalCode, Country }.Where(p => !string.IsNullOrWhiteSpace(p)));
}
