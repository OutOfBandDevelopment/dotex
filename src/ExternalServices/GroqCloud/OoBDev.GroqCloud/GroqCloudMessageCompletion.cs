using OoBDev.AI;
using OoBDev.AI.Models;
using GroqNet;
using System.Threading.Tasks;

namespace OoBDev.GroqCloud;

/// <summary>
/// Represents a class responsible for generating message completions using the GroqCloud API.
/// </summary>
public class GroqCloudMessageCompletion : IMessageCompletion
{
    private readonly GroqClient _client;
    private readonly IGroqCloudModelMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroqCloudMessageCompletion"/> class.
    /// </summary>
    /// <param name="client">The GroqCloudApiClient instance used for communication with the GroqCloud API.</param>
    /// <param name="mapper">Model mapper for GroqCloud request/response.</param>
    public GroqCloudMessageCompletion(
        GroqClient client,
        IGroqCloudModelMapper mapper
        )
    {
        _client = client;
        _mapper = mapper;
    }

    /// <summary>
    /// Generates a completion for the given prompt using the specified model.
    /// </summary>
    /// <param name="modelName">The name of the model to use for completion.</param>
    /// <param name="prompt">The prompt for which completion is requested.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the completion response.</returns>
    public async Task<string> GetCompletionAsync(string modelName, string prompt) =>
         (await this.GetCompletionAsync(new CompletionRequest() { Prompt = prompt, Model = modelName })).Response;

    /// <summary>
    /// Retrieves a completion for the given prompt from the specified model.
    /// </summary>
    /// <param name="model">Completion request model</param>
    /// <returns>Resulting response object</returns>
    public async Task<CompletionResponse> GetCompletionAsync(CompletionRequest model) =>
        _mapper.Map(
            await _client.GetChatCompletionsAsync(
                _mapper.Map(new CompletionRequest()
                {
                    Prompt = model.Prompt,
                    Model = model.Model
                }))
        );
}
