using System;

namespace OoBDev.DocumentCenter.Abstractions
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class MimeTypeAttribute : Attribute
    {
        public string MimeType { get; private set; }
        public MimeTypeAttribute(string mimeType)
        {
            MimeType = mimeType;
        }

        public override object TypeId => this;
    }
}
