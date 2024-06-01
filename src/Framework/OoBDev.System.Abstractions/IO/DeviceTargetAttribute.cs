using System;

namespace OoBDev.System.IO;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DeviceTargetAttribute(Type target) : Attribute
{
    public Type Target { get; } = target;
}