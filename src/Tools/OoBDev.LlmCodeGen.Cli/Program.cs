using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OoBDev.GroqCloud;
using OoBDev.Ollama;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.LlmCodeGen.Cli;

/// <summary>
/// Entry point of the LlmCodeGen tool.
/// </summary>
public static class Program
{
    private const string OllamaSection = "OoBDev:Ollama";
    private const string GroqSection = "OoBDev:GroqCloud";

    private static readonly Dictionary<string, string> _switches = new()
    {
        ["--input"] = $"{LlmCodeGenOptions.SectionName}:{nameof(LlmCodeGenOptions.InputPath)}",
        ["--output"] = $"{LlmCodeGenOptions.SectionName}:{nameof(LlmCodeGenOptions.OutputPath)}",
        ["--template"] = $"{LlmCodeGenOptions.SectionName}:{nameof(LlmCodeGenOptions.Template)}",
        ["--provider"] = $"{LlmCodeGenOptions.SectionName}:{nameof(LlmCodeGenOptions.Provider)}",
        ["--model"] = $"{LlmCodeGenOptions.SectionName}:{nameof(LlmCodeGenOptions.Model)}",
        ["--pattern"] = $"{LlmCodeGenOptions.SectionName}:{nameof(LlmCodeGenOptions.SearchPattern)}",
        ["--overwrite"] = $"{LlmCodeGenOptions.SectionName}:{nameof(LlmCodeGenOptions.Overwrite)}",
        ["--extract"] = $"{LlmCodeGenOptions.SectionName}:{nameof(LlmCodeGenOptions.ExtractFiles)}",
    };

    private static async Task Main(string[] args) =>
        await Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{OllamaSection}:{nameof(OllamaApiClientOptions.Url)}"] = "http://localhost:11434",
                    [$"{OllamaSection}:{nameof(OllamaApiClientOptions.DefaultModel)}"] = "phi3",
                });
                config.AddCommandLine(args, _switches);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddOptions<LlmCodeGenOptions>()
                    .Bind(context.Configuration.GetSection(LlmCodeGenOptions.SectionName))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

                services.TryAddOllamaServices(context.Configuration, OllamaSection);
                services.TryAddGroqCloudServices(context.Configuration, GroqSection);

                services.AddHostedService<LlmCodeGenService>();
            })
            .Build()
            .RunAsync();
}
