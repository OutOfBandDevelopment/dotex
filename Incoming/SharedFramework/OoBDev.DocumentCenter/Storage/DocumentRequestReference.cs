using OoBDev.DocumentCenter.Contracts.Storage;

namespace OoBDev.DocumentCenter.Storage
{
    internal class DocumentRequestReference : IDocumentRequestReference
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Key { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string? Container { get; set; }
        public string? FileName { get; set; }
    }
}
