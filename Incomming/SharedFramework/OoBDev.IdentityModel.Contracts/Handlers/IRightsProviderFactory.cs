using System.Collections.Generic;

namespace OoBDev.IdentityModel.Contracts.Handlers
{
    public interface IRightsProviderFactory
    {
        IEnumerable<IRightsProvider> GetRightsProviders();
    }
}
