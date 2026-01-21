using OoBDev.Communications.Models;
using OoBDev.Communications.Services;
using OoBDev.MessageQueueing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Controller for handling communication-related operations, such as sending emails and messages to a queue.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CommunicationsController"/> class.
/// </remarks>
/// <param name="email">The email communication sender.</param>
/// <param name="queue">The message queue sender.</param>
[ApiController]
[Route("api/[controller]")]
public class CommunicationsController(
    ICommunicationSender<EmailMessageModel> email,
    IMessageQueueSender<EmailMessageModel> queue
    ) : ControllerBase
{
    /// <summary>
    /// Sends an email publicly without requiring authentication.
    /// </summary>
    /// <param name="model">The email message model.</param>
    /// <returns>A task representing the asynchronous operation and containing a string result.</returns>
    [HttpPost("public")]
    [AllowAnonymous]
    public async Task<string> SendAnonymous([FromBody] EmailMessageModel model) =>
        await email.SendAsync(model);

    /// <summary>
    /// Enqueues an email message to be processed asynchronously.
    /// </summary>
    /// <param name="model">The email message model.</param>
    /// <returns>A task representing the asynchronous operation and containing a string result.</returns>
    [HttpPost("queued")]
    [AllowAnonymous]
    public async Task<string> Enqueue([FromBody] EmailMessageModel model) =>
        await queue.SendAsync(model, model.ReferenceId);

    /// <summary>
    /// Sends an email with authorization, requiring the caller to be authenticated.
    /// </summary>
    /// <param name="model">The email message model.</param>
    /// <returns>A task representing the asynchronous operation and containing a string result.</returns>
    [HttpPost("private")]
    [Authorize]
    public async Task<string> SendAuthorize([FromBody] EmailMessageModel model) =>
        await email.SendAsync(model);
}
