namespace OoBDev.PostgreSql.WorkQueue;

/// <summary>
/// Chooses which due items a claim takes first. The returned text is placed into the claim query as an ORDER BY list,
/// so it must come from code, never from user input.
/// </summary>
public interface IPostgresClaimOrder
{
    /// <summary>The ORDER BY expressions for a stage; columns are those of the tasks table.</summary>
    /// <param name="stage">The stage name.</param>
    /// <returns>For example <c>updated, item_key</c>.</returns>
    string OrderBy(string stage);
}
