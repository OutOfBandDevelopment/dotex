using System.Globalization;
using System.Text.Json.Serialization;

namespace OoBDev.UiDefinitions;

/// <summary>The data type a <see cref="ValueConstraint"/> checks a value against — independent of the widget that collects it.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ValueKind>))]
public enum ValueKind
{
    /// <summary>Any text; <see cref="ValueConstraint.Minimum"/>/<see cref="ValueConstraint.Maximum"/> are ignored.</summary>
    Text,

    /// <summary>A whole number (e.g. <c>"5"</c>, <c>"-3"</c>, <c>"1e3"</c>) — normalized to plain digits.</summary>
    Integer,

    /// <summary>Any real number (e.g. <c>"5"</c>, <c>"0.25"</c>, <c>"1e-3"</c>) — normalized to an invariant-culture number.</summary>
    Number,
}
