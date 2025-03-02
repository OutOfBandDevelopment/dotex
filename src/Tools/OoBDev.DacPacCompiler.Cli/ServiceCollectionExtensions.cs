using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.DacPacCompiler.Cli;

public static class ServiceCollectionExtensions
{
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
