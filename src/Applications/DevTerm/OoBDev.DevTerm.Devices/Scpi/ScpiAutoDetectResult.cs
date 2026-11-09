using System;
using OoBDev.Scpi;

namespace OoBDev.DevTerm.Devices.Scpi;

/// <summary>The result of <see cref="ScpiAutoDetect.DetectAsync"/>.</summary>
/// <param name="Outcome">How the detection ended.</param>
/// <param name="Profile">The matched profile, when <paramref name="Outcome"/> is <see cref="ScpiAutoDetectOutcome.Matched"/>.</param>
/// <param name="Reply">The <c>*IDN?</c> reply text, when one arrived.</param>
public sealed record ScpiAutoDetectResult(ScpiAutoDetectOutcome Outcome, ScpiInstrumentProfile? Profile, string? Reply)
{
    /// <summary>The message shown to the user for this outcome.</summary>
    /// <param name="timeout">The wait that was used.</param>
    /// <returns>One sentence.</returns>
    public string Describe(TimeSpan timeout) => Outcome switch
    {
        ScpiAutoDetectOutcome.Matched => $"Detected {Profile!.Name} (*IDN? replied \"{Reply}\").",
        ScpiAutoDetectOutcome.Unrecognized => $"No loaded SCPI profile recognizes *IDN? reply \"{Reply}\" — opening the Generic panel.",
        ScpiAutoDetectOutcome.NoReply => $"No *IDN? reply within {ScpiAutoDetect.FormatSeconds(timeout)} — opening the Generic panel.",
        _ => "SCPI auto-detect needs the \"scpi\" presenter — opening the Generic panel.",
    };
}
