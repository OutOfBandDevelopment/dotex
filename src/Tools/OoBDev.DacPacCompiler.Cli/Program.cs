using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OoBDev.Extensions.Configuration;
using System.Threading.Tasks;

namespace OoBDev.DacPacCompiler.Cli;

internal class Program
{
    private static async Task Main(string[] args) =>
        await Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) => config.AddCommandLine(args,
                    CommandLine.BuildParameters<DacPacBuilderEngineOptions>()
                    ))
            .ConfigureServices((context, services) =>
            {
                services.Configure<DacPacBuilderEngineOptions>(options => context.Configuration.Bind(nameof(DacPacBuilderEngineOptions), options));
                services.AddHostedService<DacPacBuilderEngineService>();
            })
            .StartAsync();
}
