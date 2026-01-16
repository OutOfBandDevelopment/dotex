using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotChat.HostRunner;

public class KernelRunner : IRunner
{
    public const string KernelName = "BotChatKernel";

    private readonly Kernel _kernel;
    private readonly ILogger _logger;
    private readonly ILogger _user;
    private readonly ILogger _assistant;

    public class User { }
    public class Assistant { }

    public KernelRunner(
        [FromKeyedServices(KernelName)] Kernel kernel,
        ILogger<KernelRunner> logger,
        ILogger<User> user,
        ILogger<Assistant> assistant
        )
    {
        _kernel = kernel;
        _logger = logger;
        _user = user;
        _assistant = assistant;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting!");

        var chatHistory = new ChatHistory();
        var history = new List<(string role, object result)>();

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var executionSettings = new OllamaPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        try
        {

            while (true)
            {
                Console.Write("User> ");
                var prompt = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(prompt))
                {
                    Console.Write("<type /done to exit>");
                    continue;
                }
                else if (string.Compare(prompt, "/done", ignoreCase: true) == 0 || string.Compare(prompt, "/exit", ignoreCase: true) == 0)
                {
                    break;
                }

                chatHistory.AddUserMessage(prompt);
                history.Add(("user", prompt));

                var result = await chatCompletionService.GetChatMessageContentAsync(
                   chatHistory,
                   executionSettings: executionSettings,
                   kernel: _kernel);

                _assistant.LogInformation("Assistant>{response}", result.Content ?? "!I know nothing!");
                chatHistory.AddAssistantMessage(result.Content ?? "!I know nothing!");
                history.Add(("assistant", result));

            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {message}", ex.Message);
            _logger.LogDebug("Error: {exception}", ex);
        }

        Console.WriteLine("--- History ---");
        foreach (var result in history)
        {
            _logger.LogInformation("({type}) {role}: {result}", result.GetType(), result.role, result);
        }
    }
}
