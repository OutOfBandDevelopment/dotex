using System;

namespace OoBDev.DocumentCenter.Contracts
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
