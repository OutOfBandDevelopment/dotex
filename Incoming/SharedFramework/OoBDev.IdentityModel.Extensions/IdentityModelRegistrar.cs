using OoBDev.IdentityModel.Contracts;
using OoBDev.IdentityModel.Contracts.Handlers;
using OoBDev.IdentityModel.Extensions.Authorization;
using OoBDev.IdentityModel.Extensions.Globalization;
using OoBDev.IdentityModel.Extensions.Services;
using OoBDev.Toolkit.Contracts.Globalization;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.IdentityModel.Extensions
{
    public class IdentityModelRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<IClaimsProvider, ClaimsProvider>();

            services.TryAddTransient<IClaimsEnhancerPipeline, ClaimsEnhancerPipeline>();
            services.TryAddTransient<IClaimsEnhancerFactory, ClaimsEnhancerFactory>();
            services.TryAddTransient<IRightsProviderFactory, RightsProviderFactory>();

            services.TryAddTransient<IUserSessionAccessor, UserSessionAccessor>();

            services.Replace(ServiceDescriptor.Singleton<ICultureInfoAccessor, ClaimsCultureInfoAccessor>());

            return services;
        }
    }
}
