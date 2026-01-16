using OoBDev.IdentityModel.Contracts;
using OoBDev.IdentityModel.Contracts.Handlers;
using System.Collections.Generic;

namespace OoBDev.IdentityModel.Extensions.Authorization
{
    public class RightsProviderFactory : IRightsProviderFactory
    {
        private readonly IEnumerable<IRightsProvider> _rights;

        public RightsProviderFactory(
            IEnumerable<IRightsProvider> rights
            )
        {
            _rights = rights;
        }

        public IEnumerable<IRightsProvider> GetRightsProviders() => _rights;
    }
}
