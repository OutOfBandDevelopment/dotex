using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using OoBDev.Scpi;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Devices.Scpi;

/// <summary>Finds the profile of the connected instrument by sending <c>*IDN?</c> through the session.</summary>
public static class ScpiAutoDetect
{
    private const string _detectReplyId = "scpiAutoDetect.reply";

    /// <summary>Sends <c>*IDN?</c> and waits for the reply to match a profile.</summary>
    /// <param name="session">The connected session.</param>
    /// <param name="presenter">The session's <c>scpi</c> presenter; without it nothing correlates the reply.</param>
    /// <param name="catalog">The profiles to match.</param>
    /// <param name="timeout">How long to wait for the reply.</param>
    /// <returns>The outcome with the matched profile and reply.</returns>
    public static async Task<ScpiAutoDetectResult> DetectAsync(Session session, IPresenter? presenter, ScpiProfileCatalog catalog, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(catalog);

        if (presenter is not IReplyTracker tracker || presenter is not IStructuredPresenter structured)
        {
            return new ScpiAutoDetectResult(ScpiAutoDetectOutcome.NoPresenter, null, null);
        }

        var replyReceived = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnValuesChanged(object? _, IReadOnlyDictionary<string, string> values)
        {
            if (values.TryGetValue(_detectReplyId, out var reply))
            {
                replyReceived.TrySetResult(reply);
            }
        }

        structured.ValuesChanged += OnValuesChanged;
        try
        {
            tracker.QuerySent(_detectReplyId);
            try
            {
                await session.SendAsync(Encoding.ASCII.GetBytes("*IDN?\n")).ConfigureAwait(false);
            }
            catch
            {
                // A reply id that will never be answered would shift every later reply onto the wrong field.
                tracker.Cancel(_detectReplyId);
                throw;
            }

            var winner = await Task.WhenAny(replyReceived.Task, Task.Delay(timeout)).ConfigureAwait(false);
            if (winner != replyReceived.Task)
            {
                tracker.Cancel(_detectReplyId);
                return new ScpiAutoDetectResult(ScpiAutoDetectOutcome.NoReply, null, null);
            }

            var idn = await replyReceived.Task.ConfigureAwait(false);
            return catalog.TryMatchByIdn(idn) is { } profile
                ? new ScpiAutoDetectResult(ScpiAutoDetectOutcome.Matched, profile, idn)
                : new ScpiAutoDetectResult(ScpiAutoDetectOutcome.Unrecognized, null, idn);
        }
        finally
        {
            structured.ValuesChanged -= OnValuesChanged;
        }
    }

    /// <summary>The status line shown while waiting.</summary>
    /// <param name="timeout">The wait.</param>
    /// <returns>One sentence.</returns>
    public static string ProgressMessage(TimeSpan timeout) =>
        $"Auto-detecting the SCPI instrument: sent *IDN?, waiting up to {FormatSeconds(timeout)}…";

    internal static string FormatSeconds(TimeSpan timeout) =>
        string.Create(CultureInfo.InvariantCulture, $"{timeout.TotalSeconds:0.#} s");
}
