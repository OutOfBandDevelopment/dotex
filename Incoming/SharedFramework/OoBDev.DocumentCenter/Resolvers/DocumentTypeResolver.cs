using OoBDev.DocumentCenter.Contracts;
using OoBDev.Toolkit.Contracts.Common;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace OoBDev.DocumentCenter.Resolvers
{
    public class DocumentTypeResolver : IDocumentTypeResolver
    {
        private readonly IEnumTools _enum;
        private readonly IGuidTools _guid;

        public DocumentTypeResolver(
            IEnumTools @enum,
            IGuidTools guid
            )
        {
            _enum = @enum;
            _guid = guid;
        }

        public string GenerateFileName(string? contentType) => GenerateFileName(this.GetByMime(contentType));

        public string GenerateFileName(DocumentTypes documentType) =>
            Convert.ToBase64String(_guid.NewGuid().ToByteArray())
                                      .TrimEnd('=')
                                      .Replace('+', '-').Replace('/', '-').Replace('=', '-')
                                      + this.GetExtension(documentType);

        public DocumentTypes GetByFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return DocumentTypes.Unknown;
            var ext = Path.GetExtension(fileName);
            var query = from i in _enum.GetAttributes<DocumentTypes, FileExtensionAttribute>()
                        where string.Equals(i.attribute.Extension, ext, StringComparison.OrdinalIgnoreCase)
                        select i.value;
            return query.FirstOrDefault();
        }

        public DocumentTypes GetByMime(string? mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType)) return DocumentTypes.Unknown;

            var simpleMimeType = mimeType.Split(';').FirstOrDefault()?.Trim();
            var query = from i in _enum.GetAttributes<DocumentTypes, MimeTypeAttribute>()
                        where string.Equals(i.attribute.MimeType, simpleMimeType, StringComparison.OrdinalIgnoreCase)
                        select i.value;
            return query.FirstOrDefault();
        }

        public DocumentTypes GetByMimeOrFileName(string? contentType, string? fileName) =>
             GetByMime(contentType) switch
             {
                 DocumentTypes.Unknown => GetByFileName(fileName),
                 DocumentTypes docType => docType
             };

        public string GetDescription(DocumentTypes documentType) =>
            _enum.GetAttributes<DescriptionAttribute>(documentType).FirstOrDefault()?.Description ?? documentType.ToString();

        public DocumentTypes GetByPackageType(PackageTypes packageTypes) =>
            _enum.GetAttributes<DocumentTypeAttribute>(packageTypes).FirstOrDefault()?.DocumentType ?? DocumentTypes.Unknown;

        public string GetExtension(DocumentTypes documentType) =>
            _enum.GetAttributes<FileExtensionAttribute>(documentType).FirstOrDefault()?.Extension ?? "";

        public string GetMimeType(DocumentTypes documentType) =>
            _enum.GetAttributes<MimeTypeAttribute>(documentType).FirstOrDefault()?.MimeType ?? "application/octet-stream";
    }
}
