using OoBDev.IdentityModel.Contracts.Claims;
using System.Collections.Generic;

namespace OoBDev.IdentityModel.Extensions.Authorization
{
    public interface IClaimsEnhancerFactory
    {
        IEnumerable<IClaimsEnhancer> GetEnhancers();
    }
}
