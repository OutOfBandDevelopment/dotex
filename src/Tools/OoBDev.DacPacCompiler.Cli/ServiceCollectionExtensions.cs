using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.DacPacCompiler.Cli;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register DACPAC compiler services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers required framework services for the DACPAC compiler.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRequiredFramework(this IServiceCollection services) => services
        //TODO: fix this up
        //.AddLogging(opt =>
        //{
        //    opt.AddConsole();
        //})
        //.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
        //    .AddEnvironmentVariables()
        //    .AddCommandLine(Environment.GetCommandLineArgs(), DacPacCompilerConfig.CommandLineSwitchMappings)
        //    .Build()
        //)
        //.AddToolkitServices()
        ;
}
