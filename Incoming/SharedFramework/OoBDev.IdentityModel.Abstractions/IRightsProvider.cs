using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Abstractions
{
    public interface IRightsProvider
    {
        Task<IEnumerable<string>> GetRightsForUserIdAsync(Guid userId);
    }
}
