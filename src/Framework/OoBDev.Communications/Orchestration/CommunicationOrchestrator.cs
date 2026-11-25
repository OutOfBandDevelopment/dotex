using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.Orchestration;

/// <summary>Reads the target's preferences, enhances the data, composes one message per preferred channel and sends it through <see cref="ISendService"/>, or holds the request.</summary>
public sealed class CommunicationOrchestrator(
    ITargetPreferenceProvider preferences,
    IEnumerable<IDataEnhancer> enhancers,
    IEnumerable<IMessageComposer> composers,
    ISendService sender,
    IDeferralStore deferrals,
    TimeProvider timeProvider) : ICommunicationOrchestrator
{
    /// <inheritdoc />
    public Task<CommunicationResult> SendAsync(CommunicationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ProcessAsync(WithCorrelation(request), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CommunicationResult> DeferAsync(CommunicationRequest request, DateTimeOffset until, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var held = WithCorrelation(request);
        await deferrals.AddAsync(new DeferredRequest(held, until), cancellationToken).ConfigureAwait(false);
        return new CommunicationResult(held.CorrelationId!, CommunicationOutcome.Deferred, $"Deferred until {until:O}", []);
    }

    /// <inheritdoc />
    public async Task<int> ReleaseDueAsync(CancellationToken cancellationToken = default)
    {
        var due = await deferrals.TakeDueAsync(timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false);
        foreach (var held in due)
        {
            await ProcessAsync(held.Request, cancellationToken).ConfigureAwait(false);
        }

        return due.Count;
    }

    private static CommunicationRequest WithCorrelation(CommunicationRequest request)
        => request.CorrelationId is null ? request with { CorrelationId = Guid.NewGuid().ToString("N") } : request;

    private async Task<CommunicationResult> ProcessAsync(CommunicationRequest request, CancellationToken cancellationToken)
    {
        var correlationId = request.CorrelationId!;
        CommunicationResult Skipped(string reason) => new(correlationId, CommunicationOutcome.Skipped, reason, []);

        var preference = await preferences.GetAsync(request.TargetId, cancellationToken).ConfigureAwait(false);
        if (preference is null)
        {
            return Skipped($"Unknown target '{request.TargetId}'");
        }

        if (request.Priority < preference.MinimumPriority)
        {
            return Skipped($"Priority {request.Priority} is below the target's minimum {preference.MinimumPriority}");
        }

        if (preference.Channels.Count == 0)
        {
            return Skipped("The target has no channels");
        }

        var now = timeProvider.GetUtcNow();
        if (request.Priority == RequestPriority.Normal && QuietHours.IsQuiet(preference, now))
        {
            var until = QuietHours.NextEnd(preference, now);
            await deferrals.AddAsync(new DeferredRequest(request, until), cancellationToken).ConfigureAwait(false);
            return new CommunicationResult(correlationId, CommunicationOutcome.Deferred, $"Quiet hours until {until:O}", []);
        }

        var data = new Dictionary<string, object?>(request.Data, StringComparer.Ordinal);
        var enhanced = request with { Data = data };
        foreach (var enhancer in enhancers.Where(e => e.AppliesTo(request.MessageType)))
        {
            await enhancer.EnhanceAsync(enhanced, data, cancellationToken).ConfigureAwait(false);
        }

        var options = new SendOptions { CorrelationId = correlationId };
        var results = new List<SendResult>();
        foreach (var channel in preference.Channels)
        {
            var composer = composers.FirstOrDefault(c => string.Equals(c.ChannelName, channel, StringComparison.OrdinalIgnoreCase));
            var message = composer is null ? null : await composer.ComposeAsync(enhanced, preference, cancellationToken).ConfigureAwait(false);
            if (message is null)
            {
                continue;
            }

            results.Add(await sender.SendAsync(channel, message, options, cancellationToken).ConfigureAwait(false));
        }

        return results.Count == 0
            ? Skipped("No channel produced a message")
            : new CommunicationResult(correlationId, CommunicationOutcome.Sent, null, results);
    }
}
