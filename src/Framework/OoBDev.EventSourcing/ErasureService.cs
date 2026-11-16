using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// GDPR erasure by tombstone and compaction (<c>docs/design/CqrsEventSourcing/erasure.md</c>): <see cref="RequestAsync"/> appends
/// an <see cref="ErasureRequested"/> tombstone to the subject's stream; <see cref="CompactAsync"/> blanks the marked fields of
/// every event of each open request's subject, appends <see cref="ErasureCompleted"/> and tells the read models. Running it twice
/// changes nothing the second time. Only this class uses <see cref="IEventPayloadRewriter"/>; a host should expose it behind its own right.
/// </summary>
public sealed class ErasureService(
    IEventStore store,
    IEventPayloadRewriter rewriter,
    EventTypeRegistry registry,
    IEnumerable<ISubjectErasureHandler> handlers)
{
    private const string RequestedType = "ErasureRequested";
    private const string CompletedType = "ErasureCompleted";
    private const int PageSize = 500;

    /// <summary>Gets the erasure stream of a subject.</summary>
    /// <param name="subjectId">The data subject.</param>
    /// <returns>The stream id.</returns>
    public static string StreamOf(string subjectId) => "erasure:" + subjectId;

    /// <summary>Appends the tombstone for a subject.</summary>
    /// <param name="subjectId">The data subject.</param>
    /// <param name="requestId">The request id.</param>
    /// <param name="knownCopies">Other places that hold copies and need their own request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task RequestAsync(string subjectId, string requestId, IReadOnlyList<string>? knownCopies = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        var payload = JsonSerializer.Serialize(new ErasureRequested(subjectId, requestId, knownCopies ?? []));
        var stream = StreamOf(subjectId);
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var last = (await store.ReadAsync(stream, 1, cancellationToken).ConfigureAwait(false)).Count;
            var result = await store.AppendAsync(stream, last, [new NewEvent(RequestedType, payload)], cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
                return;
        }

        throw new EventStreamConflictException(stream, 0);
    }

    /// <summary>Completes every open request.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of requests completed.</returns>
    public async Task<int> CompactAsync(CancellationToken cancellationToken = default)
    {
        var open = await FindOpenAsync(cancellationToken).ConfigureAwait(false);
        if (open.Count == 0)
            return 0;

        var bySubject = open.ToDictionary(r => r.SubjectId, StringComparer.Ordinal);
        var changes = new Dictionary<string, Dictionary<long, string>>(StringComparer.Ordinal);
        var counts = open.ToDictionary(r => r.RequestId, _ => 0, StringComparer.Ordinal);
        await foreach (var record in ReadEverythingAsync(cancellationToken).ConfigureAwait(false))
        {
            var rule = registry.RuleFor(record.Type);
            if (rule is null || JsonNode.Parse(record.Payload) is not JsonObject body)
                continue;
            var subject = Find(body, rule.SubjectProperty)?.Value?.ToString();
            if (subject is null || !bySubject.TryGetValue(subject, out var request))
                continue;

            var changed = false;
            foreach (var (field, placeholder) in rule.Placeholders)
            {
                var key = Find(body, field);
                if (key is null || JsonEquals(key.Value.Value, placeholder))
                    continue;
                body[key.Value.Key] = placeholder?.DeepClone();
                changed = true;
            }

            if (!changed)
                continue;
            if (!changes.TryGetValue(record.StreamId, out var stream))
                changes[record.StreamId] = stream = [];
            stream[record.Sequence] = body.ToJsonString();
            counts[request.RequestId]++;
        }

        foreach (var (streamId, payloads) in changes)
            await rewriter.RewritePayloadsAsync(streamId, payloads, cancellationToken).ConfigureAwait(false);

        foreach (var request in open)
        {
            var stream = StreamOf(request.SubjectId);
            var last = (await store.ReadAsync(stream, 1, cancellationToken).ConfigureAwait(false)).Count;
            var payload = JsonSerializer.Serialize(new ErasureCompleted(request.SubjectId, request.RequestId, counts[request.RequestId]));
            await store.AppendAsync(stream, last, [new NewEvent(CompletedType, payload)], cancellationToken).ConfigureAwait(false);
            foreach (var handler in handlers)
                await handler.SubjectErasedAsync(request.SubjectId, cancellationToken).ConfigureAwait(false);
        }

        return open.Count;
    }

    private async Task<List<ErasureRequested>> FindOpenAsync(CancellationToken cancellationToken)
    {
        var requested = new Dictionary<string, ErasureRequested>(StringComparer.Ordinal);
        var completed = new HashSet<string>(StringComparer.Ordinal);
        await foreach (var record in ReadEverythingAsync(cancellationToken).ConfigureAwait(false))
        {
            if (record.Type == RequestedType && JsonSerializer.Deserialize<ErasureRequested>(record.Payload, JsonOptions) is { } r)
                requested[r.RequestId] = r;
            else if (record.Type == CompletedType && JsonSerializer.Deserialize<ErasureCompleted>(record.Payload, JsonOptions) is { } c)
                completed.Add(c.RequestId);
        }

        return [.. requested.Values.Where(r => !completed.Contains(r.RequestId))];
    }

    private async IAsyncEnumerable<EventRecord> ReadEverythingAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        long after = 0;
        while (true)
        {
            var page = await store.ReadAllAsync(after, PageSize, cancellationToken).ConfigureAwait(false);
            if (page.Count == 0)
                yield break;
            foreach (var record in page)
                yield return record;
            after = page[^1].Position;
        }
    }

    private static KeyValuePair<string, JsonNode?>? Find(JsonObject body, string name)
    {
        foreach (var pair in body)
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                return pair;
        return null;
    }

    private static bool JsonEquals(JsonNode? a, JsonNode? b) => a is null ? b is null : b is not null && a.ToJsonString() == b.ToJsonString();

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
}
