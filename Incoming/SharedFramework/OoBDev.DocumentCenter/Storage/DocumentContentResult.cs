using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Storage;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.DocumentCenter.Storage
{
    [ExcludeFromCodeCoverage]
    internal class DocumentContentResult : IDocumentContentResult
    {
        public byte[]? Content { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string ContentType { get; set; }

        public string FileName { get; set; }

        public DocumentTypes DocumentType { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}