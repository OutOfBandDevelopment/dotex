using System.ComponentModel.DataAnnotations;

namespace OoBDev.WebApi.Models;

/// <summary>An address to look up.</summary>
/// <param name="Line1">Street line.</param>
/// <param name="City">City.</param>
/// <param name="Region">State or region.</param>
/// <param name="PostalCode">Postal code.</param>
public sealed record GeocodeRequestModel([property: Required] string Line1, string? City = null, string? Region = null, string? PostalCode = null);

/// <summary>The outcome of a lookup.</summary>
/// <param name="Matched">Whether the service found the address.</param>
/// <param name="Latitude">Latitude in degrees.</param>
/// <param name="Longitude">Longitude in degrees.</param>
/// <param name="Quality">Match quality name.</param>
/// <param name="NormalizedAddress">The address as the service normalized it.</param>
public sealed record GeocodeResponseModel(bool Matched, double? Latitude, double? Longitude, string Quality, string? NormalizedAddress);
