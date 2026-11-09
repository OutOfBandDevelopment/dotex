using System;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.Models;

/// <summary>Converts the transport payload <see cref="EmailMessageModel"/> to the sendable <see cref="EmailMessage"/>.</summary>
public static class EmailMessageModelExtensions
{
    /// <summary>Maps the model; <c>ReferenceId</c>, else <paramref name="correlationId"/>, becomes the <c>MessageId</c> so a repeat of the same request is not sent twice.</summary>
    /// <param name="model">The payload.</param>
    /// <param name="correlationId">The correlation id used when the model has no reference id.</param>
    /// <returns>The email message.</returns>
    public static EmailMessage ToEmailMessage(this EmailMessageModel model, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        return new EmailMessage
        {
            MessageId = model.ReferenceId ?? correlationId ?? Guid.NewGuid().ToString("N"),
            CorrelationId = correlationId ?? model.ReferenceId,
            From = model.FromAddress,
            To = [.. model.ToAddresses],
            Cc = [.. model.CcAddresses],
            Bcc = [.. model.BccAddresses],
            ReplyTo = model.Headers.TryGetValue("Reply-To", out var replyTo) ? replyTo : null,
            Subject = model.Subject ?? string.Empty,
            TextContent = model.TextContent,
            HtmlContent = model.HtmlContent,
        };
    }
}
