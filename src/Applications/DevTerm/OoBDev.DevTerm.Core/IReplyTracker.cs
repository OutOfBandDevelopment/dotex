namespace OoBDev.DevTerm;

/// <summary>
/// Correlates an outbound query command with the next complete reply line back over the transport:
/// a control surface calls <see cref="QuerySent"/> right before sending a query so the presenter
/// (see <see cref="LineReplyPresenter"/>) knows which indicator id the next line belongs to. The
/// synchronous, one-command-at-a-time case.
/// </summary>
public interface IReplyTracker
{
    void QuerySent(string replyIndicatorId);

    /// <summary>
    /// Removes a pending id previously registered via <see cref="QuerySent"/> — call this when the
    /// query that registered it turns out not to be getting a reply after all (its send failed, or
    /// an auto-detect-style wait timed out), so it doesn't stay queued forever and shift every later
    /// reply onto the wrong field. A no-op if the id isn't (or is no longer) pending.
    /// </summary>
    void Cancel(string replyIndicatorId);
}
