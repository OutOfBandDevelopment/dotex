using System;

namespace OoBDev.DocumentCenter.Abstractions
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class FileExtensionAttribute : Attribute
    {
        public string Extension { get; private set; }
        public FileExtensionAttribute(string extension)
        {
            Extension = extension;
        }

        public override object TypeId => this;
    }
}
