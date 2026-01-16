using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Extensions.Authorization
{

    public class ClaimsEnhancerPipeline : IClaimsEnhancerPipeline
    {
        private readonly IClaimsEnhancerFactory _factory;
        public ClaimsEnhancerPipeline(
            IClaimsEnhancerFactory factory
            )
        {
            _factory = factory;
        }
        public async Task<JObject> EnhanceAsync(JObject claims)
        {
            var enhancers = _factory.GetEnhancers().ToArray();
            foreach (var enhancer in enhancers)
            {
                claims = await enhancer.EnhanceAsync(claims ?? new JObject());
            }
            return claims;
        }
    }
}
