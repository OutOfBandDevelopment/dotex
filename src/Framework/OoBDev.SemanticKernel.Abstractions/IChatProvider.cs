using System.Threading.Tasks;

namespace OoBDev.SemanticKernel;

public interface IChatProvider
{
    Task<string?> OneShotAsync(string prompt);
}
