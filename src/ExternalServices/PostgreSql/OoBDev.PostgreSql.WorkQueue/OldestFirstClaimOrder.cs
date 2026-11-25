namespace OoBDev.PostgreSql.WorkQueue;

/// <summary>The default claim order: the item that has waited longest first.</summary>
public sealed class OldestFirstClaimOrder : IPostgresClaimOrder
{
    /// <inheritdoc />
    public string OrderBy(string stage) => "COALESCE(next_attempt_at, updated), item_key";
}
