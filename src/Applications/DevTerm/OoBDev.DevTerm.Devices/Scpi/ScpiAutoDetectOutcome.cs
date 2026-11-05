namespace OoBDev.DevTerm.Devices.Scpi;

/// <summary>How an <c>*IDN?</c> auto-detect ended.</summary>
public enum ScpiAutoDetectOutcome
{
    /// <summary>A loaded profile recognized the reply.</summary>
    Matched,

    /// <summary>The instrument replied but no profile recognizes it.</summary>
    Unrecognized,

    /// <summary>No reply arrived in time.</summary>
    NoReply,

    /// <summary>The session has no <c>scpi</c> presenter to correlate the reply.</summary>
    NoPresenter,
}
