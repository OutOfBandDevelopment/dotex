using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.MessageQueueing;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.WebApi.Controllers;

/// <summary>Sends emails through the <c>email</c> channel, directly or through the message queue, and polls the <c>inbox</c> channel (IMAP).</summary>
/// <param name="send">The send service.</param>
/// <param name="queue">The message queue sender.</param>
/// <param name="receive">The receive service that polls the inbox.</param>
[ApiController]
[Route("api/[controller]")]
public class CommunicationsController(
    ISendService send,
    IMessageQueueSender<EmailMessageModel> queue,
    IReceiveService receive
    ) : ControllerBase
{
    private const string EmailChannel = "email";
    private const string InboxChannel = "inbox";
    private const int PreviewLength = 160;

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

    /// <summary>Fetches unread mail from the <c>inbox</c> channel; the messages are handled and then marked seen, so a second poll returns only newer mail.</summary>
    /// <param name="max">The most messages to fetch (1 to 50).</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The fetched messages, or a problem when the inbox channel is missing or the mail server is unreachable.</returns>
    [HttpPost("inbox/poll")]
    [ApplicationRight("communications.receive")]
    [ProducesResponseType<IReadOnlyList<InboxMessageModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<IReadOnlyList<InboxMessageModel>>> PollInbox([FromQuery] int max = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var polled = await receive.PollAsync(InboxChannel, Math.Clamp(max, 1, 50), cancellationToken);
            return polled.OfType<IEmailMessage>().Select(ToModel).ToList();
        }
        catch (Exception ex) when (ex is ChannelNotFoundException or ProviderNotFoundException or NotSupportedException)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status404NotFound, title: "Inbox channel not available");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status502BadGateway, title: "Mail server not reachable");
        }
    }

    private static InboxMessageModel ToModel(IEmailMessage message)
    {
        var text = message.TextContent ?? string.Empty;
        return new InboxMessageModel
        {
            MessageId = message.MessageId,
            From = message.From,
            To = message.To,
            Subject = message.Subject,
            Preview = text.Length <= PreviewLength ? text : text[..PreviewLength],
            Timestamp = message.Timestamp,
        };
    }

    private async Task<ActionResult<SendResult>> SendAsync(EmailMessageModel model, CancellationToken cancellationToken)
    {
        var result = await send.SendAsync(EmailChannel, model.ToEmailMessage(), cancellationToken: cancellationToken);
        return result.Success
            ? result
            : Problem(result.Failure?.Message, statusCode: StatusCodes.Status502BadGateway, title: $"Email not sent ({result.Failure?.Kind})");
    }
}
