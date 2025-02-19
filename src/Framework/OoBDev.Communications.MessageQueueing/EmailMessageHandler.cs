using OoBDev.Communications.Models;
using OoBDev.Communications.Services;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OoBDev.Communications.MessageQueueing;

/// <summary>
/// Represents a message handler for handling and sending email messages.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EmailMessageHandler"/> class.
/// </remarks>
public class EmailMessageHandler : IMessageQueueHandler<EmailMessageModel, EmailMessageModel>
{
    private readonly ILogger _logger;
    private readonly ICommunicationSender<EmailMessageModel>? _email;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailMessageHandler"/> class.
    /// </summary>
    /// <param name="email">The communication sender for email messages.</param>
    /// <param name="logger">The logger.</param>
    public EmailMessageHandler(
        ILogger<EmailMessageHandler> logger,
        ICommunicationSender<EmailMessageModel>? email = null
    )
    {
        _logger = logger;

        if (email == null)
        {
            _logger.LogWarning($"No provider for type {nameof(ICommunicationSender<EmailMessageModel>)}");
        }
        _email = email;
    }


    /// <summary>
    /// Handles the specified email message asynchronously.
    /// </summary>
    /// <param name="message">The email message to handle.</param>
    /// <param name="context">The message context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task HandleAsync(EmailMessageModel message, IMessageContext context)
    {
        if (_email == null)
        {
            //TODO: updated process to support not handing events
            _logger.LogError(
                $"No provider for type {nameof(ICommunicationSender<EmailMessageModel>)}. Unable to send {{subject}} for {{from}} [{{id}}] as provider is not configured",
                message.Subject,
                message.FromAddress,
                message.ReferenceId
                );
            return Task.CompletedTask;
        }

        message.ReferenceId ??= context.CorrelationId;
        _logger.LogInformation("Sending {subject} for {from} [{id}]", message.Subject, message.FromAddress, message.ReferenceId);
        return _email.SendAsync(message);
    }

    /// <summary>
    /// Handles the specified message asynchronously.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="context">The message context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task HandleAsync(object message, IMessageContext context) =>
        message is EmailMessageModel messageModel ? HandleAsync(messageModel, context) : Task.CompletedTask;
}
