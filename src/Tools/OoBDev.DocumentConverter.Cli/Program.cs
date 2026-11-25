using OoBDev.Common;
using OoBDev.Common.Extensions;
using OoBDev.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace OoBDev.DocumentConverter.Cli;

/// <summary>
/// Entry point for the document converter CLI tool.
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
            global::System.Console.WriteLine(CommandLine.Usage("file-convert", "Converts a document from one format to another.", typeof(DocumentConverterOptions)));
            return 0;
        }

        await RunHostAsync(args);
        return 0;
    }

    private static async Task RunHostAsync(string[] args) =>
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
                services.TryCommonExternalExtensions(context.Configuration, new(), new() { IncludeWkHtmlToPdf = true, IncludeHtmlToOpenXml = true });
            })
            .StartAsync();
}
