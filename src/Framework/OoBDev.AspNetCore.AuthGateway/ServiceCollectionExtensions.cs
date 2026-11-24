using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System;
using System;

namespace OoBDev.AspNetCore.AuthGateway;

public static class ServiceCollectionExtensions
{
    public const string DefaultSection = "OoBDev:AuthGateway";

    /// <summary>Registers the gateway services and binds <see cref="AuthGatewayOptions"/> (validated at startup). Add YARP separately with <c>AddReverseProxy()</c>.</summary>
    public static IServiceCollection TryAddAuthGateway(this IServiceCollection services, IConfiguration configuration, string configurationSectionName = DefaultSection)
    {
        services.AddValidatedOptions<AuthGatewayOptions>(configuration, configurationSectionName);
        services.AddHttpClient(OAuthTokenProvider.HttpClientName);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IOutboundTokenProvider, OAuthTokenProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInboundAuthenticator, ApiKeyAuthenticator>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInboundAuthenticator, ClientCertificateAuthenticator>());
        services.TryAddSingleton<AuthGatewayAuthorizer>();
        return services;
    }
}
