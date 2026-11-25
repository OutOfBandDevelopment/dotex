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
        var tool = DacPacMergeHost.SelectTool(args);

        if (CommandLine.IsHelpRequest(args))
        {
            global::System.Console.WriteLine(CommandLine.Usage("dacpactools", "Builds a DACPAC from a SQL CLR assembly.", typeof(DacPacBuilderEngineOptions)));
            global::System.Console.WriteLine();
            global::System.Console.WriteLine(DacPacMergeHost.Usage);
            return 0;
        }

        switch (tool)
        {
            case DacPackTools.SqlClr:
                await RunHostAsync(args);
                return 0;
            case DacPackTools.Merge:
                await DacPacMergeHost.Create(args).StartAsync();
                return 0;
            default:
                global::System.Console.Error.WriteLine("Unknown --tool value; use SqlClr or Merge.");
                return 2;
        }
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
