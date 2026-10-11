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
public partial class LlmCodeGenService(
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
            LogDone(log, sent);
        }
        catch (OperationCanceledException)
        {
            LogCancelled(log);
        }
        catch (Exception ex)
        {
            LogFailed(log, ex);
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "done: {Count} prompts sent")]
    private static partial void LogDone(ILogger logger, int count);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "cancelled")]
    private static partial void LogCancelled(ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "llmcodegen failed")]
    private static partial void LogFailed(ILogger logger, Exception ex);
}
