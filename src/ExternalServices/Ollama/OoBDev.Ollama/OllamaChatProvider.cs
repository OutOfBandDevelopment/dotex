using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OoBDev.SemanticKernel;
using System.Threading.Tasks;

namespace OoBDev.Ollama;

public class OllamaChatProvider : IChatProvider
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly ILogger _logger;

    public OllamaChatProvider(
        [FromKeyedServices(KernelGlobal.Name)] Kernel kernel,
        ILogger<OllamaChatProvider> logger
        )
    {
        _kernel = kernel;
        _chat = kernel.GetRequiredService<IChatCompletionService>();
        _logger = logger;
    }

    public async Task<string?> OneShotAsync(string prompt)
    {
        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage(@"
Always do the following.
- Only build responses using only the data by the requested functions.
- When records are returned generate a table using markdown
- When no records are returned simply return ""No results found""
");

        chatHistory.AddUserMessage(prompt);

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var executionSettings = new OllamaPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        var result = await _chat.GetChatMessageContentAsync(
           chatHistory,
           executionSettings: executionSettings,
           kernel: _kernel);

        return result.Content;
    }
}
