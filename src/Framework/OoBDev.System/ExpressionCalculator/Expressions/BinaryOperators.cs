namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Enum representing the available binary operators in the expression calculator.
/// </summary>
public enum BinaryOperators
{
    /// <summary>
    /// Represents an unknown operator. Used as a default or fallback.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents the power operator (e.g., x^y).
    /// </summary>
    Power,

    /// <summary>
    /// Represents the multiplication operator (e.g., x * y).
    /// </summary>
    Multiply,

    /// <summary>
    /// Represents the division operator (e.g., x / y).
    /// </summary>
    Divide,

    /// <summary>
    /// Represents the modulo operator (e.g., x % y).
    /// </summary>
    Modulo,

    /// <summary>
    /// Represents the addition operator (e.g., x + y).
    /// </summary>
    Add,

    /// <summary>
    /// Represents the subtraction operator (e.g., x - y).
    /// </summary>
    Subtract,
}
