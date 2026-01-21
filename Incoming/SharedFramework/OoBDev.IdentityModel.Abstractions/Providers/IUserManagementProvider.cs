using OoBDev.Toolkit.DependencyInjection;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Abstractions.Providers
{
    [ContractConfig]
    public interface IUserManagementProvider
    {
        Task<UserCreatedModel> CreateAccountAsync(UserCreateModel model);
    }
}
