using System;

namespace OoBDev.System.IO;

/// <summary>
/// Initializes a new instance of the <see cref="DeviceTargetAttribute"/> class.
/// </summary>
/// <param name="target">The type of the target device.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DeviceTargetAttribute(Type target) : Attribute
{
    /// <summary>
    /// Gets the type of the target device.
    /// </summary>
    public Type Target { get; } = target;
}