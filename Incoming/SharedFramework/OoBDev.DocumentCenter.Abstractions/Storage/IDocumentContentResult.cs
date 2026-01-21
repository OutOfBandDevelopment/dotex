namespace OoBDev.DocumentCenter.Abstractions.Storage
{
    public interface IDocumentContentResult
    {
        byte[]? Content { get; }
        string ContentType { get; }
        string FileName { get; }
        DocumentTypes DocumentType { get; }
    }
}
