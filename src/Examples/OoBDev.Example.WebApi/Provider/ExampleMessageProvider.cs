using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Provider;

/// <summary>
/// Implementation of <see cref="IExampleMessageProvider"/> and <see cref="IMessageQueueHandler{TMessage}"/>
/// for handling and sending example messages.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ExampleMessageProvider"/> class.
/// </remarks>
/// <param name="sender">The message queue sender for sending messages.</param>
/// <param name="logger">The logger for logging messages.</param>
public partial class ExampleMessageProvider(
    IMessageQueueSender<ExampleMessageProvider> sender,
    ILogger<ExampleMessageProvider> logger) : IExampleMessageProvider, IMessageQueueHandler<ExampleMessageProvider>
{
    /// <summary>
    /// Posts an example message asynchronously.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>The message ID.</returns>
    public async Task<string> PostAsync(object message, string? correlationId)
    {
        LogSend(message, correlationId);
        var messageId = await sender.SendAsync(message, correlationId);
        LogSent(message, correlationId, messageId);
        return messageId;
    }

    /// <summary>
    /// Handles an example message asynchronously.
    /// </summary>
    /// <param name="message">The received message.</param>
    /// <param name="context">The message context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task HandleAsync(object message, IMessageContext context)
    {
        LogReceived(message, context.CorrelationId);
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Send: {message} [{correlationId}]")]
    private partial void LogSend(object message, string? correlationId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Sent: {message} [{correlationId}]-[{messageId}]")]
    private partial void LogSent(object message, string? correlationId, string messageId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Received: {message} [{correlationId}]")]
    private partial void LogReceived(object message, string? correlationId);
}
