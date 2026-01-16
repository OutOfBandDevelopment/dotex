using OoBDev.DocumentCenter.Contracts.Storage;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.DocumentCenter.Storage
{
    [ExcludeFromCodeCoverage]
    internal class DocumentStoreResult : IDocumentStoreResult
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Key { get; set; }
        public string Container { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}