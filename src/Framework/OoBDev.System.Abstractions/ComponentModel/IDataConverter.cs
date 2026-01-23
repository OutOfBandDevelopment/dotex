using System;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Provides methods for converting data between different types.
/// </summary>
public interface IDataConverter
{
    /// <summary>
    /// Converts the specified value to the target type.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value, or <c>null</c> if conversion fails.</returns>
    T? ConvertTo<T>(object? value);

    /// <summary>
    /// Converts the specified value to the specified type, with an optional default value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="type">The target type to convert to.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The converted value, or the default value if conversion fails.</returns>
    object? ConvertTo(object? value, Type type, object? defaultValue = null);

    /// <summary>
    /// Determines whether the specified value is considered null.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> if the value is null or represents a null value; otherwise, <c>false</c>.</returns>
    bool IsNull(object? value);
}
