using OoBDev.Extensions;
using OoBDev.TextTemplating.Contracts;
using OoBDev.Toolkit.Contracts.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Composers
{
    public class EmailMessageComposerConfig : IEmailMessageComposerConfig
    {
        public const string TraceConfig = "OoBDev:Communications:EmailMessageComposer:EnableTracing";
        public const string TemplateResource = "EmailMessageComposerTrace.txt";

        private readonly IConfiguration _config;
        private readonly IGenerateText _generate;

        public EmailMessageComposerConfig(
            IConfiguration config,
            IGenerateText generate
            )
        {
            _config = config;
            _generate = generate;
        }

        public bool EnableTracing => _config[TraceConfig].ToBoolean();

        public async Task<string> TracingTemplateAsync(JObject data)
        {
            var template = await this.GetResourceAsStringAsync(TemplateResource).ConfigureAwait(false);
            var generated = await _generate.GenerateAsync(template, data).ConfigureAwait(false);
            return generated ?? "";
        }
    }
}