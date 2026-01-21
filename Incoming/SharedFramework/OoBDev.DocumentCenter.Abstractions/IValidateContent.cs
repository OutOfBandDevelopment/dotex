using System.IO;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Abstractions
{
    public interface IValidateContent
    {
        Task EnsureValidContentAsync(Stream content, string fileName, string contentType);
    }
}
