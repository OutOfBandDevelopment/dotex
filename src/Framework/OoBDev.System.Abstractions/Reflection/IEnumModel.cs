using System;
using System.Collections.Generic;

namespace OoBDev.System.Reflection;
/// <summary>
/// Represents a generic interface for providing information about an enumeration value.
/// </summary>
public interface IEnumModel
{
    /// <summary>
    /// Gets the code associated with the enumeration value.
    /// </summary>
    string? Code { get; }

    /// <summary>
    /// Gets the description associated with the enumeration value.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the numerical identifier of the enumeration value.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets a value indicating whether the enumeration value is an end state.
    /// </summary>
    bool IsEndState { get; }

    /// <summary>
    /// Gets a value indicating whether the enumeration value is excluded from uniqueness checks.
    /// </summary>
    bool IsExcludeFromUnique { get; }

    /// <summary>
    /// Gets the name of the enumeration value.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the order of the enumeration value.
    /// </summary>
    int? Order { get; }

    /// <summary>
    /// Gets the underlying value of the enumeration.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Gets a collection of possible names for the enumeration value.
    /// </summary>
    IReadOnlyCollection<string> PossibleNames { get; }
}

/// <summary>
/// Represents a generic interface for providing information about an enumeration value of type <typeparamref name="TEnum"/>.
/// </summary>
/// <typeparam name="TEnum">The enumeration type.</typeparam>
public interface IEnumModel<TEnum> : IEnumModel
    where TEnum : struct, Enum
{
    /// <summary>
    /// Gets the strongly-typed value of the enumeration.
    /// </summary>
    new TEnum Value { get; }
}
