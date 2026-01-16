using OoBDev.Globalization;
using OoBDev.TextTemplating.Contracts;
using OoBDev.Toolkit.Templating.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OoBDev.TextTemplating.Templating
{
    public class TemplateResolver : ITemplateResolver
    {
        private readonly ITextTemplateProvider _provider;
        private readonly ILogger<TemplateResolver> _log;

        public TemplateResolver(
            ITextTemplateProvider provider,
            ILogger<TemplateResolver> log
            )
        {
            _provider = provider;
            _log = log;
        }

        public async Task<string> GetTemplateAsync(string templateName, CultureInfo? culture)
        {
            var (language, country) = (culture ?? CultureInfo.CurrentCulture).Split();
            var template = await _provider.GetAsync(templateName, language, country).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(template))
            {
                _log.LogWarning($@"No template found for ""{templateName}"" ({culture})");
            }
            return template;
        }

        public async Task<IEnumerable<TemplateSummaryModel>> GetTemplateSummariesAsync() =>
            await _provider.GetSummaryAsync().ConfigureAwait(false);
    }
}
