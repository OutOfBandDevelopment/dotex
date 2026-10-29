using System;
using System.Globalization;

namespace OoBDev.UiDefinitions;

/// <summary>The outcome of <see cref="ValueValidator.Validate(ValueConstraint?, string?)"/>.</summary>
public sealed class ValueValidationResult
{
    private ValueValidationResult(bool isValid, string value, string? error)
    {
        IsValid = isValid;
        Value = value;
        Error = error;
    }

    public bool IsValid { get; }

    /// <summary>The normalized value to send when <see cref="IsValid"/>; the original input otherwise.</summary>
    public string Value { get; }

    /// <summary>Why the input was rejected (e.g. <c>"'abc' is not a number."</c>); null when valid.</summary>
    public string? Error { get; }

    public static ValueValidationResult Valid(string value) => new(true, value, null);

    public static ValueValidationResult Invalid(string input, string error) => new(false, input, error);
}
