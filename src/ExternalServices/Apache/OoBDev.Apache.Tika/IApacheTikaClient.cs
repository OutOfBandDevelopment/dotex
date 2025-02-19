using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika;

public interface IApacheTikaClient
{
    ValueTask<string> DetectStreamAsync(Stream source);
    Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType);
    Task<string> GetHelloAsync();
}
