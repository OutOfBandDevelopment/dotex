using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Transports.Tcp;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the TCP transport and its options. Callers still need to bind/configure
    /// <see cref="TcpTransportOptions"/> (mode, host, port) themselves.
    /// </summary>
    public static IServiceCollection AddTcpTransport(this IServiceCollection services)
    {
        services.AddSingleton<IValidateOptions<TcpTransportOptions>, TcpTransportOptionsValidator>();
        services.AddOptions<TcpTransportOptions>().ValidateOnStart();
        services.AddSingleton<ITcpConnectionSource, SystemTcpConnectionSource>();
        services.AddTransient<ITransport, TcpTransport>();
        return services;
    }
}
