using System;

namespace OoBDev.AspNetCore.Extensions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class OoBDevInternalAttribute : Attribute
{
}
