using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace OoBDev.System.Text.Templating.EntityFrameworkCore;

/// <summary>Registers the database template source.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the template tables, the <see cref="EfTemplateRepository"/> and an <see cref="ITemplateSource"/> over them.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Picks the database provider and connection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddEntityFrameworkTemplates(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        services.AddDbContextFactory<TemplatesDbContext>(configure);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<EfTemplateRepository>();
        services.AddOptions<EfTemplateSourceOptions>();
        services.AddSingleton<ITemplateSource, EfTemplateSource>();
        return services;
    }
}
