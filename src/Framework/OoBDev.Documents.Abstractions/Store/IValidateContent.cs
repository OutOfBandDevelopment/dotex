using System.Threading.Tasks;
using OoBDev.Documents.Models;
namespace OoBDev.Documents.Store;

/// <summary>Checks a document before it is stored.</summary>
public interface IValidateContent
{
    /// <summary>Throws <see cref="InvalidContentException"/> when the content must not be stored. The stream position is restored afterwards by the store.</summary>
    /// <param name="content">The document.</param>
    Task ValidateAsync(ContentReference content);
}
