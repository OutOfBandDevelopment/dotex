using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Extensions.Authorization
{
    public interface IClaimsEnhancerPipeline
    {
        Task<JObject> EnhanceAsync(JObject claims);
    }
}
