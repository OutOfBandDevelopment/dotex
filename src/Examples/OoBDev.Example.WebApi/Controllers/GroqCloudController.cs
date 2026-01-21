using OoBDev.AI;
using OoBDev.AI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Controller for AI-related operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class GroqCloudController : ControllerBase
{
    private readonly IMessageCompletion _completion;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIController"/> class with the specified dependencies.
    /// </summary>
    /// <param name="completion">The completion provider.</param>
    public GroqCloudController(
        [FromKeyedServices("GroqCloud")] IMessageCompletion completion
        )
    {
        _completion = completion;
    }

    /// <summary>
    /// executes a completion request
    /// </summary>
    /// <param name="model">completion request</param>
    /// <returns>completion result</returns>
    [HttpPost("Completion")]
    public async Task<CompletionResponse> Completion([FromBody] CompletionRequest model) =>
        await _completion.GetCompletionAsync(model);
}
