using System;

namespace OoBDev.MessageQueueing;

/// <summary>
/// Attribute used to mark a class as a message queue handler and provide configuration options.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageQueueAttribute"/> class with the specified simple name.
/// </remarks>
/// <param name="simpleName">The simple name used for named targeting in the configuration value.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class MessageQueueAttribute(string? simpleName) : Attribute
{

    /// <summary>
    /// Gets the simple name used for named targeting in the configuration value.
    /// </summary>
    public string? SimpleName { get; } = simpleName;

    /// <summary>
    /// Gets a unique identifier for this attribute.
    /// </summary>
    public override object TypeId => this;
}
