using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OoBDev.WorkQueue;

/// <summary>
/// A process-local <see cref="IWorkQueue"/> with the same claim, lease, fence and back-off rules as the database providers.
/// Use it for tests, development and single-process tools; nothing survives a restart.
/// </summary>
/// <param name="options">The work queue options.</param>
/// <param name="clock">The time source.</param>
public sealed class InMemoryWorkQueue(IOptions<WorkQueueOptions> options, TimeProvider clock) : IWorkQueue
{
    private const int ErrorLength = 500;

    private readonly Lock _gate = new();
    private readonly Dictionary<(string Stage, string Key), Row> _rows = [];
    private readonly List<DeadRecord> _dead = [];
    private readonly WorkQueueOptions _options = options.Value;

    /// <inheritdoc />
    public Task<bool> EnqueueAsync(string stage, string key, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = clock.GetUtcNow();
            return Task.FromResult(_rows.TryAdd((stage, key), new Row(stage, key, now)));
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkItem>> ClaimAsync(string stage, string worker, int batch, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = clock.GetUtcNow();
            var sla = _options.For(stage);

            foreach (var row in StageRows(stage).Where(r => r.Attempts >= sla.MaxAttempts && IsDue(r, sla, now)).ToList())
            {
                var hung = row.Status == WorkStatus.Running;
                row.Status = hung ? WorkStatus.Parked : WorkStatus.Dead;
                row.LeaseUntil = null;
                row.Detail = hung ? $"parked: hung, lease expired after {row.Attempts} attempts" : "dead: attempts exhausted";
                row.Updated = now;
                _dead.Add(new DeadRecord(stage, row.Key, row.Attempts, hung ? "lease expired" : "attempts exhausted", null, row.ClaimedBy, now));
            }

            var claimed = StageRows(stage)
                .Where(r => r.Attempts < sla.MaxAttempts && IsDue(r, sla, now))
                .OrderBy(r => r.NextAttemptAt ?? r.Updated)
                .ThenBy(r => r.Key, StringComparer.Ordinal)
                .Take(batch)
                .ToList();

            var items = new List<WorkItem>(claimed.Count);
            foreach (var row in claimed)
            {
                row.Status = WorkStatus.Running;
                row.Attempts++;
                row.ClaimedBy = worker;
                row.ClaimedAt = now;
                row.Updated = now;
                row.LeaseUntil = now + sla.Lease;
                row.NextAttemptAt = null;
                items.Add(new WorkItem(stage, row.Key, row.Attempts, worker, row.LeaseUntil.Value));
            }

            return Task.FromResult<IReadOnlyList<WorkItem>>(items);
        }
    }

    /// <inheritdoc />
    public Task<bool> HeartbeatAsync(WorkItem item, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = clock.GetUtcNow();
            var sla = _options.For(item.Stage);
            if (!Held(item, out var row) || row.ClaimedAt + sla.MaxRuntime <= now)
                return Task.FromResult(false);

            row.LeaseUntil = now + sla.Lease;
            row.Updated = now;
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<bool> CompleteAsync(WorkItem item, string status = WorkStatus.Done, string? detail = null, CancellationToken cancellationToken = default)
    {
        if (status is not (WorkStatus.Done or WorkStatus.Skipped))
            throw new ArgumentOutOfRangeException(nameof(status));

        lock (_gate)
        {
            if (!Held(item, out var row))
                return Task.FromResult(false);

            row.Status = status;
            row.Detail = detail;
            row.LeaseUntil = null;
            row.Updated = clock.GetUtcNow();
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<string?> FailAsync(WorkItem item, string error, bool retryable = true, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!Held(item, out var row))
                return Task.FromResult<string?>(null);

            var now = clock.GetUtcNow();
            var dies = !retryable || item.Attempt >= _options.For(item.Stage).MaxAttempts;
            var text = error.Length <= ErrorLength ? error : error[..ErrorLength];

            row.Status = dies ? WorkStatus.Dead : WorkStatus.Pending;
            row.Detail = text;
            row.LeaseUntil = null;
            row.NextAttemptAt = dies ? null : now + _options.Backoff(item.Attempt);
            row.Updated = now;

            if (dies)
                _dead.Add(new DeadRecord(item.Stage, item.Key, item.Attempt, retryable ? "attempts exhausted" : "non-retryable", text, item.Worker, now));

            return Task.FromResult<string?>(row.Status);
        }
    }

    /// <inheritdoc />
    public Task<int> RedriveAsync(string stage, string? key = null, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = clock.GetUtcNow();
            var back = StageRows(stage)
                .Where(r => r.Status is WorkStatus.Dead or WorkStatus.Parked && (key is null || r.Key == key))
                .ToList();

            foreach (var row in back)
            {
                row.Status = WorkStatus.Pending;
                row.Attempts = 0;
                row.LeaseUntil = null;
                row.NextAttemptAt = null;
                row.ClaimedBy = null;
                row.Detail = "redriven from dead letters";
                row.Updated = now;
            }

            foreach (var record in _dead.Where(d => d.Stage == stage && d.RedrivenAt is null && (key is null || d.Key == key)))
                record.RedrivenAt = now;

            return Task.FromResult(back.Count);
        }
    }

    /// <inheritdoc />
    public Task<StageStats> StatsAsync(string stage, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = clock.GetUtcNow();
            var sla = _options.For(stage);
            var rows = StageRows(stage).ToList();
            return Task.FromResult(new StageStats(
                rows.Count(r => r.Attempts < sla.MaxAttempts && IsDue(r, sla, now)),
                rows.Count(r => r.Status == WorkStatus.Running),
                rows.Count(r => r.Status == WorkStatus.Running && LeaseOver(r, sla, now)),
                rows.Count(r => r.Status == WorkStatus.Parked),
                rows.Count(r => r.Status == WorkStatus.Dead)));
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<DeadLetter>> DeadLettersAsync(string stage, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<DeadLetter> list = _dead
                .Where(d => d.Stage == stage)
                .OrderByDescending(d => d.DiedAt)
                .Select(d => new DeadLetter(d.Stage, d.Key, d.Attempts, d.Reason, d.LastError, d.ClaimedBy, d.DiedAt, d.RedrivenAt))
                .ToList();
            return Task.FromResult(list);
        }
    }

    private IEnumerable<Row> StageRows(string stage) => _rows.Values.Where(r => r.Stage == stage);

    private bool Held(WorkItem item, out Row row) =>
        _rows.TryGetValue((item.Stage, item.Key), out row!)
        && row.Status == WorkStatus.Running
        && row.Attempts == item.Attempt
        && row.ClaimedBy == item.Worker;

    private static bool LeaseOver(Row row, StageSla sla, DateTimeOffset now) =>
        (row.LeaseUntil ?? row.Updated + sla.Lease) <= now || row.ClaimedAt + sla.MaxRuntime <= now;

    private static bool IsDue(Row row, StageSla sla, DateTimeOffset now) =>
        row.Status == WorkStatus.Pending && (row.NextAttemptAt is null || row.NextAttemptAt <= now)
        || row.Status == WorkStatus.Running && LeaseOver(row, sla, now);

    private sealed class Row(string stage, string key, DateTimeOffset created)
    {
        public string Stage { get; } = stage;
        public string Key { get; } = key;
        public string Status { get; set; } = WorkStatus.Pending;
        public int Attempts { get; set; }
        public string? ClaimedBy { get; set; }
        public DateTimeOffset? ClaimedAt { get; set; }
        public DateTimeOffset? LeaseUntil { get; set; }
        public DateTimeOffset? NextAttemptAt { get; set; }
        public string? Detail { get; set; }
        public DateTimeOffset Updated { get; set; } = created;
    }

    private sealed record DeadRecord(string Stage, string Key, int Attempts, string Reason, string? LastError, string? ClaimedBy, DateTimeOffset DiedAt)
    {
        public DateTimeOffset? RedrivenAt { get; set; }
    }
}
