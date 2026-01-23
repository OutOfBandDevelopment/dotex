namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Defines the unary operators supported by the expression calculator.
/// </summary>
public enum UnaryOperators
{
    /// <summary>
    /// Unknown or unsupported unary operator.
    /// </summary>
    Unknown,

    /// <summary>
    /// Negation operator (-), which inverts the sign of a number.
    /// </summary>
    Negate,

    /// <summary>
    /// Factorial operator (!), which computes the product of all positive integers less than or equal to the operand.
    /// </summary>
    Factorial,
}
