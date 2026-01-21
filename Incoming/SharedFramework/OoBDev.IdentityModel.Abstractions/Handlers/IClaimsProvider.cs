using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Abstractions.Handlers
{
    public interface IClaimsProvider
    {
        Task<JObject> GetAdditionalClaimsAsync(JObject claims);
    }
}
