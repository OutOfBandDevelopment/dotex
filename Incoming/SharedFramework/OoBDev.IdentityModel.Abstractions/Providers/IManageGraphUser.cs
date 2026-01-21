using System;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Abstractions.Providers
{
    public interface IManageGraphUser
    {
        Task<(string objectId, string? password)> CreateGraphUserAsync(string email, string firstName, string lastName);

        Task<bool> RemoveGraphUserAsync(string userId );

    }
}
