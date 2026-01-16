using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Contracts
{
    public interface IRightsProvider
    {
        Task<IEnumerable<string>> GetRightsForUserIdAsync(Guid userId);
    }
}
