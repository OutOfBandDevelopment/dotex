using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OoBDev.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using OoBDev.DacFx;

namespace OoBDev.DacPacCompiler.Cli;

internal sealed class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (CommandLine.IsHelpRequest(args))
        {
            global::System.Console.WriteLine(CommandLine.Usage("oobdev-dacpac", "Builds a DACPAC from a SQL CLR assembly.", typeof(DacPacBuilderEngineOptions)));
            return 0;
        }

        await RunHostAsync(args);
        return 0;
    }

    private static async Task RunHostAsync(string[] args) =>
        await Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) => config.AddCommandLine(args,
                    CommandLine.BuildParameters<DacPacBuilderEngineOptions>()
                    ).AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                    }))
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(options => options.AddFilter("Microsoft", LogLevel.Warning));
                services.Configure<DacPacBuilderEngineOptions>(options => context.Configuration.Bind(nameof(DacPacBuilderEngineOptions), options));
                services.TryAddDacPacServices();
                services.AddHostedService<DacPacBuilderEngineService>();
            })
            .StartAsync();
}
