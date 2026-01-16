using OoBDev.TextTemplating.Contracts;
using OoBDev.TextTemplating.Templating;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.TextTemplating
{
    public class TextTemplatingRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<IGenerateText, GenerateText>();
            services.TryAddTransient<ITemplateResolver, TemplateResolver>();
            return services;
        }
    }
}
