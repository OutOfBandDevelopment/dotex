using System;
using System.Collections.Generic;

namespace OoBDev.System.ExpressionCalculator.Expressions;

/// <summary>
/// Represents a variable expression that references a named variable in the expression calculator.
/// The variable's value is looked up in the variables dictionary during evaluation.
/// </summary>
/// <typeparam name="T">The numeric type of the variable, which must be a value type implementing IComparable&lt;T&gt; and IEquatable&lt;T&gt;.</typeparam>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
public sealed class VariableExpression<T> : ExpressionBase<T>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    where T : struct, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// Gets the name of the variable referenced by this expression.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the VariableExpression class with the specified variable name.
    /// </summary>
    /// <param name="name">The name of the variable. Must not be null or whitespace.</param>
    /// <exception cref="InvalidOperationException">Thrown when the variable name is null or whitespace.</exception>
    public VariableExpression(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Variable name not assigned");
        Name = name;
    }

    /// <summary>
    /// Creates a copy of this variable expression with the same variable name.
    /// </summary>
    /// <returns>A new VariableExpression instance with the same variable name.</returns>
    public override ExpressionBase<T> Clone() => new VariableExpression<T>(Name);

    /// <summary>
    /// Evaluates this expression by looking up the variable's value in the provided variables dictionary.
    /// </summary>
    /// <param name="variables">The dictionary of variable names to values.</param>
    /// <returns>The value associated with this variable's name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the variable name is not found in the dictionary.</exception>
    public override T Evaluate(IDictionary<string, T> variables) => variables[Name];

    /// <summary>
    /// Returns the variable name as the string representation of this expression.
    /// </summary>
    /// <returns>The variable name.</returns>
    public override string ToString() => Name;

    /// <summary>
    /// Determines whether this variable expression is equal to another object.
    /// Supports comparison with other VariableExpression instances and raw string values.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the objects are equal (same variable name); otherwise, false.</returns>
    public override bool Equals(object? obj) =>
        this == obj ||
        obj is VariableExpression<T> vari && Name.Equals(vari.Name) ||
        obj is string && Name.Equals(obj);
}
