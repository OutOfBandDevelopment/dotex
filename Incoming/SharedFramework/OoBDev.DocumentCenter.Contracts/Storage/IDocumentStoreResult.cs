namespace OoBDev.DocumentCenter.Contracts.Storage
{
    public interface IDocumentStoreResult
    {
        string Key { get; }
        string Container { get; }
    }
}
