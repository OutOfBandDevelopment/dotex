using System;
using static OoBDev.System.ExpressionCalculator.Expressions.BinaryOperators;
using static OoBDev.System.ExpressionCalculator.Expressions.UnaryOperators;

namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Provides extension methods for converting between operator enumerations and their string representations,
/// and for querying operator properties such as associativity and priority.
/// </summary>
public static class OperatorExtensions
{
    /// <summary>
    /// Converts a unary operator to its string representation.
    /// </summary>
    /// <param name="operator">The unary operator to convert.</param>
    /// <returns>The string representation of the operator (e.g., "-" for Negate, "!" for Factorial).</returns>
    /// <exception cref="NotSupportedException">Thrown when the operator is not supported.</exception>
    public static string AsString(this UnaryOperators @operator) =>
        @operator switch
        {
            Negate => "-",
            Factorial => "!",

            _ => throw new NotSupportedException($"Operator {@operator} not supported")
        };

    /// <summary>
    /// Determines whether a unary operator is right-associative (applied on the right side of the operand).
    /// </summary>
    /// <param name="operator">The unary operator to check.</param>
    /// <returns>True if the operator is right-associative (e.g., factorial "!"); false if left-associative (e.g., negate "-").</returns>
    /// <exception cref="NotSupportedException">Thrown when the operator is not supported.</exception>
    public static bool IsRight(this UnaryOperators @operator) =>
        @operator switch
        {
            Negate => false,
            Factorial => true,

            _ => throw new NotSupportedException($"Operator {@operator} not supported")
        };

    /// <summary>
    /// Converts a string representation to a unary operator enumeration value.
    /// </summary>
    /// <param name="input">The string to parse (e.g., "-", "!").</param>
    /// <returns>The corresponding UnaryOperators enumeration value, or Unknown if not recognized.</returns>
    public static UnaryOperators AsUnaryOperator(this string input) =>
        input switch
        {
            "-" => Negate,
            "!" => Factorial,

            _ => UnaryOperators.Unknown
        };

    /// <summary>
    /// Converts a binary operator to its string representation.
    /// </summary>
    /// <param name="operator">The binary operator to convert.</param>
    /// <returns>The string representation of the operator (e.g., "+", "-", "*", "/", "%", "^").</returns>
    public static string AsString(this BinaryOperators @operator) =>
        @operator switch
        {
            Power => "^",

            Multiply => "*",
            Divide => "/",
            Modulo => "%",

            Add => "+",
            Subtract => "-",

            _ => $"?{@operator}?"
        };

    /// <summary>
    /// Converts a string representation to a binary operator enumeration value.
    /// </summary>
    /// <param name="input">The string to parse (e.g., "+", "-", "*", "/", "%", "^").</param>
    /// <returns>The corresponding BinaryOperators enumeration value, or Unknown if not recognized.</returns>
    public static BinaryOperators AsBinaryOperators(this string input) =>
        input switch
        {
            "^" => Power,

            "*" => Multiply,
            "/" => Divide,
            "%" => Modulo,

            "+" => Add,
            "-" => Subtract,

            _ => BinaryOperators.Unknown
        };

    /// <summary>
    /// Gets the precedence priority of a binary operator for expression evaluation.
    /// Higher numbers indicate higher precedence (evaluated first).
    /// Priority levels: Power (3), Multiply/Divide/Modulo (2), Add/Subtract (1).
    /// </summary>
    /// <param name="operator">The binary operator to query.</param>
    /// <returns>The priority level as an integer, or int.MaxValue for unknown operators.</returns>
    public static int GetPriority(this BinaryOperators @operator) =>
        @operator switch
        {
            Power => 3,

            Multiply => 2,
            Divide => 2,
            Modulo => 2,

            Add => 1,
            Subtract => 1,

            _ => int.MaxValue,
        };
}
