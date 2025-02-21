using OoBDev.Common;
using OoBDev.Common.Extensions;
using OoBDev.Extensions.Configuration;
using OoBDev.System.Text.Templating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace OoBDev.FileRagEngine.Cli;

public class Program
{
    private static async Task Main(string[] args) =>
        await Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) => config.AddCommandLine(args,
                    CommandLine.BuildParameters<FileRagEngineOptions>()
                               .AddParameters<FileTemplatingOptions>()
                    ))
            .ConfigureServices((context, services) =>
            {
                services.Configure<FileRagEngineOptions>(options => context.Configuration.Bind(nameof(FileRagEngineOptions), options));

                services.AddHostedService<FileRagEngineService>();

                services.TryCommonExtensions(context.Configuration, new());
                services.TryCommonExternalExtensions(context.Configuration, new(), new());
            })
            .StartAsync();
}
