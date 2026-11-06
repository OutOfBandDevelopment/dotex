using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;
using OoBDev.MessageQueueing;

namespace OoBDev.WebApi.Controllers;

/// <summary>Sends emails through the <c>email</c> channel, directly or through the message queue.</summary>
/// <param name="send">The send service.</param>
/// <param name="queue">The message queue sender.</param>
[ApiController]
[Route("api/[controller]")]
public class CommunicationsController(
    ISendService send,
    IMessageQueueSender<EmailMessageModel> queue
    ) : ControllerBase
{
    private const string EmailChannel = "email";

    /// <summary>Sends an email without requiring authentication.</summary>
    /// <param name="model">The email message model.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The send result, or a problem when the provider did not accept the message.</returns>
    [HttpPost("public")]
    [AllowAnonymous]
    [ProducesResponseType<SendResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public Task<ActionResult<SendResult>> SendAnonymous([FromBody] EmailMessageModel model, CancellationToken cancellationToken) =>
        SendAsync(model, cancellationToken);

    /// <summary>Enqueues an email message to be processed asynchronously.</summary>
    /// <param name="model">The email message model.</param>
    /// <returns>The queue message id.</returns>
    [HttpPost("queued")]
    [AllowAnonymous]
    public async Task<string> Enqueue([FromBody] EmailMessageModel model) =>
        await queue.SendAsync(model, model.ReferenceId);

    /// <summary>Sends an email; the caller must be authenticated.</summary>
    /// <param name="model">The email message model.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The send result, or a problem when the provider did not accept the message.</returns>
    [HttpPost("private")]
    [Authorize]
    [ProducesResponseType<SendResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public Task<ActionResult<SendResult>> SendAuthorize([FromBody] EmailMessageModel model, CancellationToken cancellationToken) =>
        SendAsync(model, cancellationToken);

    private async Task<ActionResult<SendResult>> SendAsync(EmailMessageModel model, CancellationToken cancellationToken)
    {
        var result = await send.SendAsync(EmailChannel, model.ToEmailMessage(), cancellationToken: cancellationToken);
        return result.Success
            ? result
            : Problem(result.Failure?.Message, statusCode: StatusCodes.Status502BadGateway, title: $"Email not sent ({result.Failure?.Kind})");
    }
}
