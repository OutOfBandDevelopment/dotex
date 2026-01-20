using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Abstractions.Claims
{
    public interface IClaimsEnhancer
    {
        Task<JObject> EnhanceAsync(JObject claims);
    }
}