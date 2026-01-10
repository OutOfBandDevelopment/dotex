using System;

namespace OoBDev.DocumentCenter.Contracts
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class DocumentTypeAttribute : Attribute
    {
        public DocumentTypes DocumentType { get; }

        public DocumentTypeAttribute(
            DocumentTypes documentType
            )
        {
            DocumentType = documentType;
        }

        public override object TypeId => this;
    }
}
