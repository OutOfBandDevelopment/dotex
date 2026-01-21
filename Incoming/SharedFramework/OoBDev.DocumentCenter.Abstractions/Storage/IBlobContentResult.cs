namespace OoBDev.DocumentCenter.Abstractions.Storage
{
    public interface IBlobContentResult
    {
        byte[]? Content { get; }
        string? ContentType { get; }
        string? FileName { get; }
    }
}