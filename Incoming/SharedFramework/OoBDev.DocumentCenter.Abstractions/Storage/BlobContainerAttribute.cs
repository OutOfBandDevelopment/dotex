using System;

namespace OoBDev.DocumentCenter.Abstractions.Storage
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BlobContainerAttribute : Attribute
    {
        public string? ContainerName { get; set; }
    }
}
