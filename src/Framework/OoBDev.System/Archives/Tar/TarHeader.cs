namespace OoBDev.System.Archives.Tar;

public record TarHeader
{
    public required string FileName { get; init; }
    public required string FileMode { get; init; }
    public required string OwnerId { get; init; }
    public required string GroupId { get; init; }
    public required int FileSize { get; init; }
    public required int LastModifiedTime { get; init; }
    public required string CheckSum { get; init; }
    public required TarFileType FileType { get; init; }
    public required string LinkedFile { get; init; }
}
