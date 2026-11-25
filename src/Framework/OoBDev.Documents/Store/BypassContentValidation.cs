using System.Threading.Tasks;
using OoBDev.Documents.Models;
namespace OoBDev.Documents.Store;

/// <summary>The default validator: accepts everything.</summary>
public sealed class BypassContentValidation : IValidateContent
{
    /// <inheritdoc />
    public Task ValidateAsync(ContentReference content) => Task.CompletedTask;
}
