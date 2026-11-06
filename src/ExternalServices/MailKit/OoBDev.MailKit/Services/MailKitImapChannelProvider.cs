using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;

namespace OoBDev.MailKit.Services;

/// <summary>Polls the IMAP inbox as the <c>mailkit-imap</c> provider of the <c>email</c> protocol; messages are marked seen only on acknowledge.</summary>
public sealed partial class MailKitImapChannelProvider(
    IImapClientFactory imap,
    IMimeMessageFactory mime,
    IOptions<MailKitImapClientOptions> options,
    TimeProvider timeProvider,
    ILogger<MailKitImapChannelProvider> log) : IPollableChannelProvider
{
    /// <inheritdoc />
    public string ProviderName => MailKitGlobals.ImapProviderName;

    /// <inheritdoc />
    public string[] SupportedProtocols { get; } = [MailKitGlobals.EmailProtocol];

    /// <inheritdoc />
    public Task<bool> CanSendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default) => Task.FromResult(false);

    /// <inheritdoc />
    public Task<SendResult> SendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return Task.FromResult(new SendResult(
            false, message.MessageId, message.CorrelationId, null, timeProvider.GetUtcNow(),
            new SendFailure(SendFailureKind.Rejected, "The IMAP provider only receives; use the mailkit provider to send.")));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PolledMessage>> PollAsync(IChannel channel, int maxMessages, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxMessages, 1);
        using var client = await imap.CreateAsync().ConfigureAwait(false);
        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken).ConfigureAwait(false);

        var ids = await inbox.SearchAsync(SearchQuery.NotDeleted.And(SearchQuery.NotSeen), cancellationToken).ConfigureAwait(false);
        var server = options.Value.Uri?.ToString() ?? $"{options.Value.Host}:{options.Value.Port}";
        var polled = new List<PolledMessage>();
        foreach (var id in ids.Take(maxMessages))
        {
            var message = await inbox.GetMessageAsync(id, cancellationToken).ConfigureAwait(false);
            var email = mime.ToReceived(message, server, inbox.Name);
            polled.Add(new PolledMessage(email, Receipt(inbox.UidValidity, id)));
        }

        await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
        LogPolled(polled.Count, channel.Name);
        return polled;
    }

    /// <inheritdoc />
    public async Task AcknowledgeAsync(IChannel channel, IReadOnlyCollection<string> receipts, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(receipts);
        if (receipts.Count == 0)
        {
            return;
        }

        using var client = await imap.CreateAsync().ConfigureAwait(false);
        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken).ConfigureAwait(false);

        var ids = receipts.Select(r => ParseReceipt(r, inbox.UidValidity)).Where(id => id is not null).Select(id => id!.Value).ToList();
        if (ids.Count > 0)
        {
            await inbox.StoreAsync(ids, new StoreFlagsRequest(StoreAction.Add, MessageFlags.Seen) { Silent = true }, cancellationToken).ConfigureAwait(false);
        }

        if (ids.Count != receipts.Count)
        {
            LogStaleReceipts(receipts.Count - ids.Count, channel.Name);
        }

        await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
    }

    private static string Receipt(uint uidValidity, UniqueId id) => $"{uidValidity}:{id.Id}";

    private static UniqueId? ParseReceipt(string receipt, uint currentValidity)
    {
        var parts = receipt.Split(':');
        return parts.Length == 2 && uint.TryParse(parts[0], out var validity) && validity == currentValidity && uint.TryParse(parts[1], out var id)
            ? new UniqueId(id)
            : null;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Polled {Count} unread email(s) on channel {Channel}")]
    private partial void LogPolled(int count, string channel);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Skipped {Count} receipt(s) on channel {Channel}: the mailbox UID validity changed or the receipt is malformed")]
    private partial void LogStaleReceipts(int count, string channel);
}
