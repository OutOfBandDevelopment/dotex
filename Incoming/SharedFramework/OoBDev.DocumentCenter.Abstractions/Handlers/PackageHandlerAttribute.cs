using System;

namespace OoBDev.DocumentCenter.Abstractions.Handlers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PackageHandlerAttribute : Attribute
    {
        public PackageTypes PackageType { get; }

        public PackageHandlerAttribute(
            PackageTypes packageType
            )
        {
            PackageType = packageType;
        }

        public int Priority { get; set; }

        public override object TypeId => this;
    }
}
