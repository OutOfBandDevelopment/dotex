using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.AI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.LlmCodeGen.Cli;

/// <summary>
/// Runs the folder loop once and stops the host.
/// </summary>
public class LlmCodeGenService(
    IOptions<LlmCodeGenOptions> options,
    IServiceProvider services,
    IHostApplicationLifetime lifetime,
    ILoggerFactory loggerFactory,
    ILogger<LlmCodeGenService> log) : BackgroundService
{
    private const string GroqAlias = "groq";
    private const string GroqKey = "groq-cloud";

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var settings = options.Value;
            var key = string.Equals(settings.Provider, GroqAlias, StringComparison.OrdinalIgnoreCase) ? GroqKey : settings.Provider;
            var completion = services.GetKeyedService<IMessageCompletion>(key)
                ?? throw new InvalidOperationException($"Provider \"{settings.Provider}\" is not available; configure OoBDev:Ollama:Url or the Groq API key.");

            var sent = await new FolderPromptRunner(completion, loggerFactory.CreateLogger<FolderPromptRunner>())
                .RunAsync(settings, stoppingToken);
            log.LogInformation("done: {Count} prompts sent", sent);
        }
        catch (OperationCanceledException)
        {
            log.LogInformation("cancelled");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "llmcodegen failed");
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }
}
