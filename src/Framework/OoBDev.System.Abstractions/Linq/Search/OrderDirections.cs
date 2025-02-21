using OoBDev.System.ComponentModel;
using OoBDev.System.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Enumeration to control sort order
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverterEx<OrderDirections>))]
public enum OrderDirections
{
    /// <summary>
    /// sort related items in ascending order
    /// </summary>
    [EnumValue(OrderDirectionsConstants.AscendingShort)]
    Ascending = 0,

    /// <summary>
    /// sort related items in descending order
    /// </summary>
    [EnumValue(OrderDirectionsConstants.DescendingShort)]
    Descending = 1,
}
