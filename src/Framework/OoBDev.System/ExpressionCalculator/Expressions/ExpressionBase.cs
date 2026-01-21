using OoBDev.System.ExpressionCalculator.Parser;
using System;
using System.Collections.Generic;

namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Provides the base class for all expression types in the expression calculator framework.
/// Supports implicit conversions to and from various numeric types and string representations.
/// </summary>
/// <typeparam name="T">The numeric type used for expression evaluation. Must be a value type that implements IComparable and IEquatable.</typeparam>
public abstract class ExpressionBase<T>
    where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Evaluates the expression using the specified variable values.
    /// </summary>
    /// <param name="variables">A dictionary mapping variable names to their values.</param>
    /// <returns>The result of evaluating the expression.</returns>
    public abstract T Evaluate(IDictionary<string, T> variables);

    /// <summary>
    /// Creates a deep copy of this expression.
    /// </summary>
    /// <returns>A new expression instance that is a clone of this expression.</returns>
    public abstract ExpressionBase<T> Clone();

    /// <summary>
    /// Implicitly converts a string to an expression by parsing it.
    /// </summary>
    /// <param name="expression">The string representation of the expression to parse.</param>
    public static implicit operator ExpressionBase<T>(string expression) =>
        new ExpressionParser<T>().Parse(expression);

    /// <summary>
    /// Implicitly converts an expression to its string representation.
    /// </summary>
    /// <param name="expression">The expression to convert to a string.</param>
    public static implicit operator string(ExpressionBase<T> expression) =>
        expression?.ToString() ?? "";

    /// <summary>
    /// Implicitly converts a decimal value to an expression.
    /// </summary>
    /// <param name="expression">The decimal value to convert.</param>
    public static implicit operator ExpressionBase<T>(decimal expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a float value to an expression.
    /// </summary>
    /// <param name="expression">The float value to convert.</param>
    public static implicit operator ExpressionBase<T>(float expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a double value to an expression.
    /// </summary>
    /// <param name="expression">The double value to convert.</param>
    public static implicit operator ExpressionBase<T>(double expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a signed byte value to an expression.
    /// </summary>
    /// <param name="expression">The signed byte value to convert.</param>
    public static implicit operator ExpressionBase<T>(sbyte expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a byte value to an expression.
    /// </summary>
    /// <param name="expression">The byte value to convert.</param>
    public static implicit operator ExpressionBase<T>(byte expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a 16-bit signed integer to an expression.
    /// </summary>
    /// <param name="expression">The 16-bit signed integer to convert.</param>
    public static implicit operator ExpressionBase<T>(short expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a 16-bit unsigned integer to an expression.
    /// </summary>
    /// <param name="expression">The 16-bit unsigned integer to convert.</param>
    public static implicit operator ExpressionBase<T>(ushort expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a 32-bit signed integer to an expression.
    /// </summary>
    /// <param name="expression">The 32-bit signed integer to convert.</param>
    public static implicit operator ExpressionBase<T>(int expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a 32-bit unsigned integer to an expression.
    /// </summary>
    /// <param name="expression">The 32-bit unsigned integer to convert.</param>
    public static implicit operator ExpressionBase<T>(uint expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a 64-bit signed integer to an expression.
    /// </summary>
    /// <param name="expression">The 64-bit signed integer to convert.</param>
    public static implicit operator ExpressionBase<T>(long expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Implicitly converts a 64-bit unsigned integer to an expression.
    /// </summary>
    /// <param name="expression">The 64-bit unsigned integer to convert.</param>
    public static implicit operator ExpressionBase<T>(ulong expression) => new ExpressionParser<T>().Parse(expression.ToString());

    /// <summary>
    /// Explicitly converts an expression to its evaluated result of type T.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>The evaluated result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the expression is null.</exception>
    public static explicit operator T(ExpressionBase<T> expression) =>
        expression?.Evaluate() ?? throw new ArgumentNullException(nameof(expression));

    /// <summary>
    /// Implicitly converts an evaluated expression to a decimal value.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator decimal(ExpressionBase<T> expression) => Convert.ToDecimal((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a float value.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator float(ExpressionBase<T> expression) => Convert.ToSingle((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a double value.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator double(ExpressionBase<T> expression) => Convert.ToDouble((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a signed byte value.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator sbyte(ExpressionBase<T> expression) => Convert.ToSByte((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a byte value.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator byte(ExpressionBase<T> expression) => Convert.ToByte((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a 16-bit signed integer.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator short(ExpressionBase<T> expression) => Convert.ToInt16((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a 16-bit unsigned integer.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator ushort(ExpressionBase<T> expression) => Convert.ToUInt16((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a 32-bit signed integer.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator int(ExpressionBase<T> expression) => Convert.ToInt32((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a 32-bit unsigned integer.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator uint(ExpressionBase<T> expression) => Convert.ToUInt32((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a 64-bit signed integer.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator long(ExpressionBase<T> expression) => Convert.ToInt64((T)expression);

    /// <summary>
    /// Implicitly converts an evaluated expression to a 64-bit unsigned integer.
    /// </summary>
    /// <param name="expression">The expression to evaluate and convert.</param>
    public static implicit operator ulong(ExpressionBase<T> expression) => Convert.ToUInt64((T)expression);
}
