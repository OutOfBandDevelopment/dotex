using OoBDev.Communications.Contracts.Composers;
using OoBDev.TextTemplating.Contracts;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Composers
{
    public class TemplateProvider : ITemplateProvider
    {
        private readonly ITemplateResolver _resolver;
        private readonly IGenerateText _generate;

        public TemplateProvider(
            ITemplateResolver resolver,
            IGenerateText generate
            )
        {
            _resolver = resolver;
            _generate = generate;
        }

        public async Task<string?> GetTemplateAsync(string messageType, string? deliveryChannel, string? section, CultureInfo? culture, JObject? data) =>
            await _generate.GenerateAsync(
                await _resolver.GetTemplateAsync(
                    GetTemplateName(messageType, deliveryChannel, section),
                    culture
                ).ConfigureAwait(false), data
            ).ConfigureAwait(false);

        private string GetTemplateName(params string?[] parts) =>
            string.Join('-', parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}