using OoBDev.Toolkit.DependencyInjection;
using System.Threading.Tasks;

namespace OoBDev.Accounting.Contracts
{
    [ContractConfig]
    public interface IAccountProvider
    {
        Task<IAccountDetail?> GetAccountDetailsByIdAsync(string externalReferenceId);
    }
}
