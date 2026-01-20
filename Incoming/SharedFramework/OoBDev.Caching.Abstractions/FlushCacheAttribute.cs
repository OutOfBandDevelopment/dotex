using System;

namespace OoBDev.Caching.Abstractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class FlushCacheAttribute : Attribute
    {
        public FlushCacheAttribute(string keyFormatter) => KeyFormatter = keyFormatter;
        public FlushCacheAttribute(Type targetClass, string methodName) => (TargetClass, MethodName) = (targetClass, methodName);

        public Type? TargetClass { get; }
        public string? MethodName { get; }
        public string? KeyFormatter { get; }

        public override object TypeId => this;
    }
}
