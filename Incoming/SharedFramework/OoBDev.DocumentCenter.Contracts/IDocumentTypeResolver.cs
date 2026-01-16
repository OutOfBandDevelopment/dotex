namespace OoBDev.DocumentCenter.Contracts
{
    public interface IDocumentTypeResolver
    {
        string GetMimeType(DocumentTypes documentType);
        string GetExtension(DocumentTypes documentType);
        string GetDescription(DocumentTypes documentType);

        DocumentTypes GetByFileName(string? fileName);
        DocumentTypes GetByMime(string? mimeType);
        DocumentTypes GetByMimeOrFileName(string? contentType, string? fileName);
        DocumentTypes GetByPackageType(PackageTypes packageTypes);

        string GenerateFileName(string? contentType);
        string GenerateFileName(DocumentTypes documentType);
    }
}
