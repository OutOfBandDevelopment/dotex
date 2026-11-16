using OoBDev.Communications.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications;

/// <summary>
/// Wraps an <see cref="ISendService"/> and removes suppressed addresses from an email before it is sent. When no
/// <c>To</c> address is left the message is not sent and the result is a <see cref="SendFailureKind.Rejected"/> failure.
/// Other message types pass through.
/// </summary>
/// <param name="inner">The real send service.</param>
/// <param name="suppressions">The suppression list.</param>
/// <param name="timeProvider">The clock for the rejected result.</param>
public class SuppressingSendService(ISendService inner, ISuppressionList suppressions, TimeProvider timeProvider) : ISendService
{
    /// <inheritdoc />
    public async Task<SendResult> SendAsync(string channelName, IMessage message, SendOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message is not IEmailMessage email)
        {
            return await inner.SendAsync(channelName, message, options, cancellationToken).ConfigureAwait(false);
        }

        var to = await AllowedAsync(email.To, cancellationToken).ConfigureAwait(false);
        if (to.Length == 0)
        {
            return new SendResult(false, message.MessageId, message.CorrelationId, null, timeProvider.GetUtcNow(),
                new SendFailure(SendFailureKind.Rejected, "Every recipient is on the suppression list."));
        }

        var filtered = new EmailMessage
        {
            MessageId = email.MessageId,
            CorrelationId = email.CorrelationId,
            Timestamp = email.Timestamp,
            Metadata = email.Metadata,
            To = to,
            Cc = await AllowedAsync(email.Cc, cancellationToken).ConfigureAwait(false),
            Bcc = await AllowedAsync(email.Bcc, cancellationToken).ConfigureAwait(false),
            From = email.From,
            ReplyTo = email.ReplyTo,
            Subject = email.Subject,
            TextContent = email.TextContent,
            HtmlContent = email.HtmlContent,
            Attachments = email.Attachments,
        };
        return await inner.SendAsync(channelName, filtered, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SendResult>> SendToMultipleChannelsAsync(IReadOnlyCollection<string> channelNames, IMessage message, SendOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channelNames);
        var results = new List<SendResult>();
        foreach (var channel in channelNames)
        {
            results.Add(await SendAsync(channel, message, options, cancellationToken).ConfigureAwait(false));
        }

        return results;
    }

    private async Task<string[]> AllowedAsync(string[] addresses, CancellationToken cancellationToken)
    {
        var allowed = new List<string>();
        foreach (var address in addresses ?? [])
        {
            if (!await suppressions.IsSuppressedAsync(address, cancellationToken).ConfigureAwait(false))
            {
                allowed.Add(address);
            }
        }

        return allowed.ToArray();
    }
}
