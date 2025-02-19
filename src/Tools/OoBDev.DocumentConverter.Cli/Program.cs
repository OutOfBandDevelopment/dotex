using OoBDev.Common;
using OoBDev.Common.Extensions;
using OoBDev.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace OoBDev.DocumentConverter.Cli;

public class Program
{
    private static async Task Main(string[] args) =>
        await Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) => config.AddCommandLine(args,
                    CommandLine.BuildParameters<DocumentConverterOptions>()
                    ))
            .ConfigureServices((context, services) =>
            {
                services.Configure<DocumentConverterOptions>(options => context.Configuration.Bind(nameof(DocumentConverterOptions), options));

                services.AddHostedService<DocumentConverterService>();
                services.AddHttpClient();

                services.TryCommonExtensions(context.Configuration, new());
                services.TryCommonExternalExtensions(context.Configuration, new(), new());
            })
            .StartAsync();
}
