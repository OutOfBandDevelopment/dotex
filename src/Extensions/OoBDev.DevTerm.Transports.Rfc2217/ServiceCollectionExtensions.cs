using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.DevTerm;
using OoBDev.DevTerm.Transports.Tcp;

namespace OoBDev.DevTerm.Transports.Rfc2217;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the RFC 2217 transport and its options. Callers still need to bind/configure
    /// <see cref="Rfc2217TransportOptions"/> (host, port, baud, ...) themselves.
    /// </summary>
    public static IServiceCollection AddRfc2217Transport(this IServiceCollection services)
    {
        services.AddOptions<Rfc2217TransportOptions>().ValidateDataAnnotations().ValidateOnStart();

        // TryAdd: idempotent if DevTerm.Transports.Tcp's own AddTcpTransport() already registered this.
        services.TryAddSingleton<ITcpConnectionSource, SystemTcpConnectionSource>();
        services.AddTransient<ITransport, Rfc2217Transport>();
        return services;
    }
}
