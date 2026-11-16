using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.SendGrid;

/// <summary>Sends <see cref="IEmailMessage"/> instances through the SendGrid v3 mail API as the <c>sendgrid</c> provider of the <c>email</c> protocol.</summary>
public sealed partial class SendGridChannelProvider(
    HttpClient http,
    IOptions<SendGridOptions> options,
    TimeProvider timeProvider,
    ILogger<SendGridChannelProvider> log) : IChannelProvider
{
    /// <inheritdoc />
    public string ProviderName => SendGridGlobals.ProviderName;

    /// <inheritdoc />
    public string[] SupportedProtocols { get; } = [SendGridGlobals.EmailProtocol];

    /// <inheritdoc />
    public Task<bool> CanSendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default)
        => Task.FromResult(message is IEmailMessage);

    /// <inheritdoc />
    public async Task<SendResult> SendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(message);
        if (message is not IEmailMessage email)
        {
            return Failed(message, SendFailureKind.Rejected, "The message is not an email.");
        }

        var settings = options.Value;
        var from = email.From ?? settings.DefaultFrom;
        if (string.IsNullOrWhiteSpace(from))
        {
            return Failed(message, SendFailureKind.Rejected, "The message has no sender and no default sender is configured.");
        }

        if (email.To.Length == 0)
        {
            return Failed(message, SendFailureKind.Rejected, "The message has no recipient.");
        }

        try
        {
            var body = await BuildAsync(email, from, cancellationToken).ConfigureAwait(false);
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(settings.BaseAddress, "v3/mail/send"))
            {
                Content = new StringContent(body.ToJsonString(), global::System.Text.Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
            using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var id = response.Headers.TryGetValues("X-Message-Id", out var values) ? values.FirstOrDefault() : null;
                LogSent(email.MessageId, channel.Name);
                return new SendResult(true, email.MessageId, email.CorrelationId, id, timeProvider.GetUtcNow());
            }

            var kind = Classify(response.StatusCode);
            LogFailed(email.MessageId, channel.Name, (int)response.StatusCode, kind);
            return new SendResult(false, email.MessageId, email.CorrelationId, null, timeProvider.GetUtcNow(),
                new SendFailure(kind, $"SendGrid answered {(int)response.StatusCode}.", (int)response.StatusCode, response.Headers.RetryAfter?.Delta));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            LogFailed(email.MessageId, channel.Name, 0, SendFailureKind.Transient);
            return Failed(message, SendFailureKind.Transient, ex.GetType().Name);
        }
    }

    internal static SendFailureKind Classify(HttpStatusCode status) => (int)status switch
    {
        429 => SendFailureKind.RateLimited,
        >= 500 => SendFailureKind.Transient,
        408 => SendFailureKind.Transient,
        _ => SendFailureKind.Permanent,
    };

    private static async Task<JsonObject> BuildAsync(IEmailMessage email, string from, CancellationToken cancellationToken)
    {
        static JsonArray Addresses(IEnumerable<string> list) => new([.. list.Select(a => (JsonNode)new JsonObject { ["email"] = a })]);

        var personalization = new JsonObject { ["to"] = Addresses(email.To) };
        if (email.Cc.Length > 0)
        {
            personalization["cc"] = Addresses(email.Cc);
        }

        if (email.Bcc.Length > 0)
        {
            personalization["bcc"] = Addresses(email.Bcc);
        }

        var content = new JsonArray();
        if (email.TextContent is not null)
        {
            content.Add(new JsonObject { ["type"] = "text/plain", ["value"] = email.TextContent });
        }

        if (email.HtmlContent is not null)
        {
            content.Add(new JsonObject { ["type"] = "text/html", ["value"] = email.HtmlContent });
        }

        if (content.Count == 0)
        {
            content.Add(new JsonObject { ["type"] = "text/plain", ["value"] = " " });
        }

        var root = new JsonObject
        {
            ["personalizations"] = new JsonArray(personalization),
            ["from"] = new JsonObject { ["email"] = from },
            ["subject"] = email.Subject,
            ["content"] = content,
        };
        if (!string.IsNullOrWhiteSpace(email.ReplyTo))
        {
            root["reply_to"] = new JsonObject { ["email"] = email.ReplyTo };
        }

        if (email.Attachments.Count > 0)
        {
            var attachments = new JsonArray();
            foreach (var attachment in email.Attachments)
            {
                var bytes = await attachment.GetContentAsync(cancellationToken).ConfigureAwait(false);
                var item = new JsonObject
                {
                    ["content"] = Convert.ToBase64String(bytes.Span),
                    ["filename"] = attachment.FileName,
                    ["type"] = attachment.ContentType,
                    ["disposition"] = attachment.ContentId is null ? "attachment" : "inline",
                };
                if (attachment.ContentId is not null)
                {
                    item["content_id"] = attachment.ContentId;
                }

                attachments.Add(item);
            }

            root["attachments"] = attachments;
        }

        return root;
    }

    private SendResult Failed(IMessage message, SendFailureKind kind, string text)
        => new(false, message.MessageId, message.CorrelationId, null, timeProvider.GetUtcNow(), new SendFailure(kind, text));

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Sent email {MessageId} on channel {Channel}")]
    private partial void LogSent(string messageId, string channel);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Email {MessageId} on channel {Channel} failed with status {Status} ({Kind})")]
    private partial void LogFailed(string messageId, string channel, int status, SendFailureKind kind);
}
