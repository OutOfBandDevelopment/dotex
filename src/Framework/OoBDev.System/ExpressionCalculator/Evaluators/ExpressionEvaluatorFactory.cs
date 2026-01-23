using System;

namespace OoBDev.System.ExpressionCalculator.Evaluators;

/// <summary>
/// Factory for creating type-specific expression evaluators.
/// </summary>
public static class ExpressionEvaluatorFactory
{
    /// <summary>
    /// Creates an expression evaluator for the specified numeric type.
    /// Supports decimal, double, float, int, short, long, sbyte, byte, uint, ushort, and ulong.
    /// </summary>
    /// <typeparam name="T">The numeric type to create an evaluator for.</typeparam>
    /// <returns>An expression evaluator for the specified type.</returns>
    /// <exception cref="NotSupportedException">Thrown when the specified type is not supported.</exception>
    public static IExpressionEvaluator<T> Create<T>()
        where T : struct, IComparable<T>, IEquatable<T> =>
            typeof(T) == typeof(decimal) ? (IExpressionEvaluator<T>)(object)new DecimalExpressionEvaluator() :

            typeof(T) == typeof(double) ? (IExpressionEvaluator<T>)(object)new DoubleExpressionEvaluator() :
            typeof(T) == typeof(float) ? (IExpressionEvaluator<T>)(object)new FloatExpressionEvaluator() :

            typeof(T) == typeof(int) ? (IExpressionEvaluator<T>)(object)new Int32ExpressionEvaluator() :
            typeof(T) == typeof(short) ? (IExpressionEvaluator<T>)(object)new Int16ExpressionEvaluator() :
            typeof(T) == typeof(long) ? (IExpressionEvaluator<T>)(object)new Int64ExpressionEvaluator() :
            typeof(T) == typeof(sbyte) ? (IExpressionEvaluator<T>)(object)new Int8ExpressionEvaluator() :

            typeof(T) == typeof(byte) ? (IExpressionEvaluator<T>)(object)new UInt8ExpressionEvaluator() :
            typeof(T) == typeof(uint) ? (IExpressionEvaluator<T>)(object)new UInt32ExpressionEvaluator() :
            typeof(T) == typeof(ushort) ? (IExpressionEvaluator<T>)(object)new UInt16ExpressionEvaluator() :
            typeof(T) == typeof(ulong) ? (IExpressionEvaluator<T>)(object)new UInt64ExpressionEvaluator() :

        throw new NotSupportedException($"Type \"{typeof(T)}\" is not supported");
}
