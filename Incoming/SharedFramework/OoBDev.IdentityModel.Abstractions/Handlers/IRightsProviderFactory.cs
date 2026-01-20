using System.Collections.Generic;

namespace OoBDev.IdentityModel.Abstractions.Handlers
{
    public interface IRightsProviderFactory
    {
        IEnumerable<IRightsProvider> GetRightsProviders();
    }
}
