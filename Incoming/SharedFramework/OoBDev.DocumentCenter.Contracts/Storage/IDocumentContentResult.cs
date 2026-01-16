namespace OoBDev.DocumentCenter.Contracts.Storage
{
    public interface IDocumentContentResult
    {
        byte[]? Content { get; }
        string ContentType { get; }
        string FileName { get; }
        DocumentTypes DocumentType { get; }
    }
}
