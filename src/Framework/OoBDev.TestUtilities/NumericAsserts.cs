using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OoBDev.TestUtilities;

/// <summary>
/// Provides assertion methods for comparing numeric values with appropriate tolerance.
/// Handles floating-point precision issues that arise from operation reordering or rounding.
/// </summary>
public static class NumericAsserts
{
    /// <summary>
    /// Asserts that two numeric values are similar within an appropriate tolerance.
    /// For floating-point types (double, float, decimal), uses epsilon-based comparison.
    /// For integer types, uses exact equality.
    /// </summary>
    /// <typeparam name="T">The numeric type being compared</typeparam>
    /// <param name="expected">The expected value</param>
    /// <param name="actual">The actual value</param>
    /// <param name="message">Optional message to display if the assertion fails</param>
    /// <remarks>
    /// Epsilon tolerance is calculated as: Max(|expected|, |actual|) × tolerance_factor
    /// - double: 1e-10 (10 decimal places)
    /// - float: 1e-5 (5 decimal places)
    /// - decimal: 0.0000000001 (10 decimal places)
    ///
    /// This approach handles cases where optimizers reorder operations, causing different
    /// but mathematically equivalent results due to floating-point rounding.
    /// </remarks>
    public static void AreSimilar<T>(T expected, T actual, string? message = null)
        where T : struct, IComparable<T>, IEquatable<T>
    {
        // Use epsilon tolerance for floating-point types to handle rounding differences
        // that occur when operations are reordered or when precision is lost
        if (typeof(T) == typeof(double))
        {
            var exp = Convert.ToDouble(expected);
            var act = Convert.ToDouble(actual);
            var epsilon = Math.Max(Math.Abs(exp), Math.Abs(act)) * 1e-10;
            var displayMessage = message ?? $"Expected: {expected}, Actual: {actual}";
            Assert.AreEqual(exp, act, epsilon, displayMessage);
        }
        else if (typeof(T) == typeof(float))
        {
            var exp = Convert.ToSingle(expected);
            var act = Convert.ToSingle(actual);
            var epsilon = Math.Max(Math.Abs(exp), Math.Abs(act)) * 1e-5f;
            var displayMessage = message ?? $"Expected: {expected}, Actual: {actual}";
            Assert.AreEqual(exp, act, epsilon, displayMessage);
        }
        else if (typeof(T) == typeof(decimal))
        {
            var exp = Convert.ToDecimal(expected);
            var act = Convert.ToDecimal(actual);
            var epsilon = Math.Max(Math.Abs(exp), Math.Abs(act)) * 0.0000000001m;
            var displayMessage = message ?? $"Expected: {expected}, Actual: {actual}";
            Assert.AreEqual(exp, act, epsilon, displayMessage);
        }
        else
        {
            // For integer types, use exact equality
            Assert.AreEqual(expected, actual, message);
        }
    }

    /// <summary>
    /// Asserts that two numeric values are similar within a specified custom tolerance.
    /// </summary>
    /// <typeparam name="T">The numeric type being compared</typeparam>
    /// <param name="expected">The expected value</param>
    /// <param name="actual">The actual value</param>
    /// <param name="tolerance">The maximum acceptable difference between values</param>
    /// <param name="message">Optional message to display if the assertion fails</param>
    public static void AreSimilar<T>(T expected, T actual, T tolerance, string? message = null)
        where T : struct, IComparable<T>, IEquatable<T>
    {
        var displayMessage = message ?? $"Expected: {expected}, Actual: {actual}, Tolerance: {tolerance}";

        if (typeof(T) == typeof(double))
        {
            var exp = Convert.ToDouble(expected);
            var act = Convert.ToDouble(actual);
            var tol = Convert.ToDouble(tolerance);
            Assert.AreEqual(exp, act, tol, displayMessage);
        }
        else if (typeof(T) == typeof(float))
        {
            var exp = Convert.ToSingle(expected);
            var act = Convert.ToSingle(actual);
            var tol = Convert.ToSingle(tolerance);
            Assert.AreEqual(exp, act, tol, displayMessage);
        }
        else if (typeof(T) == typeof(decimal))
        {
            var exp = Convert.ToDecimal(expected);
            var act = Convert.ToDecimal(actual);
            var tol = Convert.ToDecimal(tolerance);
            Assert.AreEqual(exp, act, tol, displayMessage);
        }
        else
        {
            // For integer types, convert tolerance to absolute difference check
            var exp = Convert.ToInt64(expected);
            var act = Convert.ToInt64(actual);
            var tol = Convert.ToInt64(tolerance);
            var diff = Math.Abs(exp - act);
            Assert.IsTrue(diff <= tol, displayMessage);
        }
    }
}
