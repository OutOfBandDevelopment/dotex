using System;

namespace OoBDev.Communications.Abstractions;

/// <summary>Marks an <see cref="IDataEnhancer"/> for discovery by <c>TryAddCommunicationEnhancers</c> and limits it to one message type.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CommunicationAttribute : Attribute
{
    /// <summary>Gets or sets the message type the enhancer handles (for example <c>order.confirmation</c>); null leaves the choice to <see cref="IDataEnhancer.AppliesTo"/>.</summary>
    public string? MessageType { get; set; }
}
