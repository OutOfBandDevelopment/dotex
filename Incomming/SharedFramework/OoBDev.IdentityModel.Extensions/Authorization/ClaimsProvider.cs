using OoBDev.IdentityModel.Contracts.Handlers;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Extensions.Authorization
{
    public class ClaimsProvider : IClaimsProvider
    {
        private readonly IClaimsEnhancerPipeline _claims;

        public ClaimsProvider(
             IClaimsEnhancerPipeline claims
            )
        {
            _claims = claims;
        }

        public async Task<JObject> GetAdditionalClaimsAsync(JObject claims) => await _claims.EnhanceAsync(claims);
    }
}
