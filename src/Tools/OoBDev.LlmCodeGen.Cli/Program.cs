using System;
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

    private static readonly Dictionary<string, string> _switches = new(StringComparer.Ordinal)
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

    private const string Usage = """
        oobdev-llmcodegen --input <folder> --output <folder> --template <name|file> --model <model> [--provider ollama|groq-cloud] [--pattern *.*] [--overwrite true|false] [--extract true|false]
          --input      folder scanned recursively; every folder with matching files becomes one prompt (required)
          --output     folder for prompts, responses and extracted files (required)
          --template   bundled template name (for example documentation) or a Handlebars file (required)
          --model      model name sent with each prompt (required)
          --provider   ollama (default) or groq-cloud (alias groq)
          --pattern    file pattern selecting the files in a prompt (default *.*)
          --overwrite  rerun folders that already have a response (default false)
          --extract    write files found in fenced blocks of the response beside it (default true)
        """;

    private static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && Array.Exists(args, a => a is "-h" or "--help" or "-?"))
        {
            Console.WriteLine(Usage);
            return 0;
        }

        await RunHostAsync(args);
        return 0;
    }

    private static async Task RunHostAsync(string[] args) =>
        await Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
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
