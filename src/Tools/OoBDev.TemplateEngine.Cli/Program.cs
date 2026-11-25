using OoBDev.Extensions.Configuration;
using OoBDev.Handlebars;
using OoBDev.System;
using OoBDev.System.Text.Templating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace OoBDev.TemplateEngine.Cli;

/// <summary>
/// Entry point for the Template Engine CLI tool.
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
            global::System.Console.WriteLine(CommandLine.Usage("templateengine", "Renders a Handlebars or XSLT template over input files.", typeof(TemplateEngineOptions), typeof(FileTemplatingOptions)));
            return 0;
        }

        await RunHostAsync(args);
        return 0;
    }

    private static async Task RunHostAsync(string[] args) =>
        await Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) => config.AddCommandLine(args,
                    CommandLine.BuildParameters<TemplateEngineOptions>()
                               .AddParameters<FileTemplatingOptions>()
                    ))
            .ConfigureServices((context, services) =>
            {
                services.Configure<TemplateEngineOptions>(options => context.Configuration.Bind(nameof(TemplateEngineOptions), options));

                services.AddHostedService<TemplateEngineService>();

                services.TryAddSystemExtensions(context.Configuration, new());
                services.TryAddHandlebarServices();
            })
            .StartAsync();
}
