using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Composers
{
    public interface IEmailMessageComposerConfig
    {
        bool EnableTracing { get; }

        Task<string> TracingTemplateAsync(JObject data);
    }
}