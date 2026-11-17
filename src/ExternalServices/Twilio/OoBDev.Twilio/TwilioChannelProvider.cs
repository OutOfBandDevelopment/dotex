using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Twilio;

/// <summary>Sends <see cref="ISmsMessage"/> instances through the Twilio Messages API as the <c>twilio</c> provider of the <c>sms</c> protocol.</summary>
public sealed partial class TwilioChannelProvider(
    HttpClient http,
    IOptions<TwilioOptions> options,
    TimeProvider timeProvider,
    ILogger<TwilioChannelProvider> log) : IChannelProvider
{
    /// <inheritdoc />
    public string ProviderName => TwilioGlobals.ProviderName;

    /// <inheritdoc />
    public string[] SupportedProtocols { get; } = [TwilioGlobals.SmsProtocol];

    /// <inheritdoc />
    public Task<bool> CanSendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default)
        => Task.FromResult(message is ISmsMessage);

    /// <inheritdoc />
    public async Task<SendResult> SendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(message);
        if (message is not ISmsMessage sms)
        {
            return Failed(message, SendFailureKind.Rejected, "The message is not a text message.");
        }

        var settings = options.Value;
        var from = sms.From ?? settings.DefaultFrom;
        if (string.IsNullOrWhiteSpace(from))
        {
            return Failed(message, SendFailureKind.Rejected, "The message has no sender and no default sender is configured.");
        }

        if (string.IsNullOrWhiteSpace(sms.To))
        {
            return Failed(message, SendFailureKind.Rejected, "The message has no recipient.");
        }

        try
        {
            var path = string.Create(CultureInfo.InvariantCulture, $"2010-04-01/Accounts/{Uri.EscapeDataString(settings.AccountSid)}/Messages.json");
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(settings.BaseAddress, path))
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["To"] = sms.To,
                    ["From"] = from,
                    ["Body"] = sms.Body,
                }),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.AccountSid}:{settings.AuthToken}")));
            using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var sid = await ReadSidAsync(response, cancellationToken).ConfigureAwait(false);
                LogSent(sms.MessageId, channel.Name);
                return new SendResult(true, sms.MessageId, sms.CorrelationId, sid, timeProvider.GetUtcNow());
            }

            var kind = Classify(response.StatusCode);
            LogFailed(sms.MessageId, channel.Name, (int)response.StatusCode, kind);
            return new SendResult(false, sms.MessageId, sms.CorrelationId, null, timeProvider.GetUtcNow(),
                new SendFailure(kind, $"Twilio answered {(int)response.StatusCode}.", (int)response.StatusCode, response.Headers.RetryAfter?.Delta));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            LogFailed(sms.MessageId, channel.Name, 0, SendFailureKind.Transient);
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

    private static async Task<string?> ReadSidAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false);
            return document.RootElement.TryGetProperty("sid", out var sid) ? sid.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private SendResult Failed(IMessage message, SendFailureKind kind, string text)
        => new(false, message.MessageId, message.CorrelationId, null, timeProvider.GetUtcNow(), new SendFailure(kind, text));

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Sent text {MessageId} on channel {Channel}")]
    private partial void LogSent(string messageId, string channel);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Text {MessageId} on channel {Channel} failed with status {Status} ({Kind})")]
    private partial void LogFailed(string messageId, string channel, int status, SendFailureKind kind);
}
