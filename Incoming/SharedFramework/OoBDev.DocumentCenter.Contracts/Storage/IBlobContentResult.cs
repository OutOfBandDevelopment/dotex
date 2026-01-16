namespace OoBDev.DocumentCenter.Contracts.Storage
{
    public interface IBlobContentResult
    {
        byte[]? Content { get; }
        string? ContentType { get; }
        string? FileName { get; }
    }
}