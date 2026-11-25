using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleNotificationService.Util;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Amazon.Ses;

/// <summary>Turns the SNS notifications of SES event publishing (send, delivery, bounce, complaint, reject, open, click) into <see cref="DeliveryStatusEvent"/> instances.</summary>
public sealed class SnsDeliveryWebhookProcessor(IOptions<SesOptions> options) : IWebhookProcessor
{
    /// <inheritdoc />
    public string Provider => AwsSesGlobals.ProviderName;

    /// <inheritdoc />
    public bool ValidateSignature(IChannel channel, WebhookRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var message = Message.ParseMessage(Encoding.UTF8.GetString(request.Body.Span));
            var allowed = options.Value.AllowedTopicArns;
            return (allowed.Length == 0 || allowed.Contains(message.TopicArn, StringComparer.Ordinal)) && message.IsMessageSignatureValid();
        }
        catch (Exception ex) when (ex is JsonException or AmazonClientException or ArgumentException or InvalidOperationException or FormatException or NotSupportedException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ChannelEvent>> ProcessAsync(IChannel channel, WebhookRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var envelope = JsonDocument.Parse(request.Body);
        var root = envelope.RootElement;
        if (Text(root, "Type") != "Notification" || Text(root, "Message") is not { } inner)
        {
            return Task.FromResult<IReadOnlyList<ChannelEvent>>([]);
        }

        using var payload = JsonDocument.Parse(inner);
        return Task.FromResult(Map(payload.RootElement, request.ReceivedAt));
    }

    internal static IReadOnlyList<ChannelEvent> Map(JsonElement payload, DateTimeOffset receivedAt)
    {
        var type = Text(payload, "eventType") ?? Text(payload, "notificationType");
        if (type is null || !payload.TryGetProperty("mail", out var mail))
        {
            return [];
        }

        var (status, detail) = type switch
        {
            "Send" => (DeliveryStatus.Sent, (string?)null),
            "Delivery" => (DeliveryStatus.Delivered, null),
            "Bounce" => (DeliveryStatus.Bounced, Nested(payload, "bounce", "bounceType")),
            "Complaint" => (DeliveryStatus.Complained, Nested(payload, "complaint", "complaintFeedbackType") ?? "complaint"),
            "Reject" => (DeliveryStatus.Failed, Nested(payload, "reject", "reason")),
            "Rendering Failure" => (DeliveryStatus.Failed, Nested(payload, "failure", "errorMessage")),
            "Open" => (DeliveryStatus.Opened, null),
            "Click" => (DeliveryStatus.Clicked, Nested(payload, "click", "link")),
            _ => (DeliveryStatus.Queued, "ignored"),
        };
        if (detail == "ignored" && status == DeliveryStatus.Queued)
        {
            return [];
        }

        var providerId = Text(mail, "messageId");
        var ours = mail.TryGetProperty("tags", out var tags) && tags.TryGetProperty("message-id", out var tag) && tag.ValueKind == JsonValueKind.Array && tag.GetArrayLength() > 0
            ? tag[0].GetString()
            : null;
        var id = ours ?? providerId;
        if (id is null)
        {
            return [];
        }

        var timestamp = Text(mail, "timestamp") is { } t && DateTimeOffset.TryParse(t, null, global::System.Globalization.DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : receivedAt;
        return [new DeliveryStatusEvent(id, new DeliveryStatusUpdate(id, status, detail, timestamp, Recipients(payload, status)))];
    }

    private static IReadOnlyList<string>? Recipients(JsonElement payload, DeliveryStatus status)
    {
        var (parent, list) = status switch
        {
            DeliveryStatus.Bounced => ("bounce", "bouncedRecipients"),
            DeliveryStatus.Complained => ("complaint", "complainedRecipients"),
            _ => (null, null),
        };
        if (parent is null || !payload.TryGetProperty(parent, out var section) || !section.TryGetProperty(list!, out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return items.EnumerateArray().Select(i => Text(i, "emailAddress")).OfType<string>().ToList();
    }

    private static string? Text(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static string? Nested(JsonElement element, string parent, string name)
        => element.TryGetProperty(parent, out var inner) ? Text(inner, name) : null;
}
