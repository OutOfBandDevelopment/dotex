using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Contracts.Handlers
{
    public interface IClaimsProvider
    {
        Task<JObject> GetAdditionalClaimsAsync(JObject claims);
    }
}
