using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.Composers
{
    public interface ITemplateProvider
    {
        Task<string?> GetTemplateAsync(string messageType, string? deliveryChannel, string? section, CultureInfo? culture, JObject? data);
    }
}