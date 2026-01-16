using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using OoBDev.DocumentCenter.Contracts.Storage;
using OoBDev.Toolkit.Contracts.IO;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Packaging
{
    [PackageHandler(PackageTypes.ZipFile)]
    public class ZipFileDocumentPackageHandler : IDocumentPackageHandler
    {
        private readonly IDocumentStore _store;
        private readonly ITempFileFactory _temp;

        public ZipFileDocumentPackageHandler(
            IDocumentStore store,
            ITempFileFactory temp
            )
        {
            _store = store;
            _temp = temp;
        }

        public async Task<byte[]> PackageAsync(IEnumerable<IDocumentRequestReference> documents)
        {
            using var tempFile = _temp.GetTempFile();
            using (var zip = ZipFile.Open(tempFile.FilePath, ZipArchiveMode.Update))
            {
                foreach (var document in from d in documents
                                         where d != null
                                         where !string.IsNullOrWhiteSpace(d.Key)
                                         select d)
                {
                    var content = await _store.GetAsync(document.Key, document.Container).ConfigureAwait(false);

                    if (content == null) continue;

                    var filename = new[] { document.FileName, content.FileName, document.Key }.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f));
                    var entry = zip.CreateEntry(filename);
                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(content.Content).ConfigureAwait(false);
                }
            }
            var data = await File.ReadAllBytesAsync(tempFile.FilePath);
            return data;
        }
    }
}
