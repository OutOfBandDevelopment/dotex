using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.AspNetCore.Authorization;
using OoBDev.System;
using System;

namespace OoBDev.AspNetCore.EntityFrameworkCore;

/// <summary>Registers the durable right audit trail.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the buffered sink, the database store and the writer that flushes and purges. Call
    /// <c>AddRightAuditLog()</c> afterwards (it wraps the checker and hands each check to the sink).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Binds <c>OoBDev:RightAudit</c>.</param>
    /// <param name="configure">Picks the database provider and connection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddEntityFrameworkRightAudit(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        services.AddValidatedOptions<RightAuditOptions>(configuration, "OoBDev:RightAudit");
        services.AddDbContextFactory<RightAuditDbContext>(configure);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<EfRightAuditStore>();
        services.TryAddSingleton<BufferedRightAuditSink>();
        services.TryAddSingleton<IRightAuditSink>(sp => sp.GetRequiredService<BufferedRightAuditSink>());
        services.AddHostedService<RightAuditWriter>();
        return services;
    }
}
