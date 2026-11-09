using OoBDev.Common;
using OoBDev.Common.Extensions;
using OoBDev.Extensions.Configuration;
using OoBDev.System.Text.Templating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace OoBDev.FileRagEngine.Cli;

/// <summary>
/// Entry point for the File RAG Engine CLI tool.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A task representing the application lifetime.</returns>
    private static async Task<int> Main(string[] args)
    {
        if (CommandLine.IsHelpRequest(args))
        {
            global::System.Console.WriteLine(CommandLine.Usage("oobdev-filerag", "Runs a template over files with a language model.", typeof(FileRagEngineOptions), typeof(FileTemplatingOptions)));
            return 0;
        }

        await RunHostAsync(args);
        return 0;
    }

    private static async Task RunHostAsync(string[] args) =>
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
