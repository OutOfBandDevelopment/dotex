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
public partial class EmailMessageHandler : IMessageQueueHandler<EmailMessageModel, EmailMessageModel>
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
            LogNoProvider();
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
            LogUnableToSend(message.Subject, message.FromAddress, message.ReferenceId);
            return Task.CompletedTask;
        }

        message.ReferenceId ??= context.CorrelationId;
        LogSending(message.Subject, message.FromAddress, message.ReferenceId);
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

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "No provider for type ICommunicationSender<>")]
    private partial void LogNoProvider();

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "No provider for type ICommunicationSender<>. Unable to send {subject} for {from} [{id}] as provider is not configured")]
    private partial void LogUnableToSend(string? subject, string? from, string? id);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Sending {subject} for {from} [{id}]")]
    private partial void LogSending(string? subject, string? from, string? id);
}
