namespace OoBDev.DocumentCenter.Abstractions.Storage
{
    public interface IDocumentStoreResult
    {
        string Key { get; }
        string Container { get; }
    }
}
