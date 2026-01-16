using OoBDev.DocumentCenter.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Storage
{
    public class BypassValidateContent : IValidateContent
    {
        public Task EnsureValidContentAsync(Stream content, string fileName, string contentType) => Task.CompletedTask;
    }
}