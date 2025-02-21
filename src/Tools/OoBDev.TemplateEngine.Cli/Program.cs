using OoBDev.Extensions.Configuration;
using OoBDev.Handlebars;
using OoBDev.System;
using OoBDev.System.Text.Templating;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace OoBDev.TemplateEngine.Cli;

public class Program
{
    private static async Task Main(string[] args) =>
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
