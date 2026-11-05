using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>
/// A family of instruments whose panel is picked from data rather than hand-coded per device (SCPI is the one today): the
/// Device menu gets one item, which asks which instrument (or auto-detects it), then opens that instrument's panel. A plugin
/// registers one next to its presenter; the front ends never name the plugin's project. See
/// docs/design/Plugins/plugin-contributed-panels.md. For a fixed single-device panel use <see cref="IDevicePanelContribution"/>.
/// </summary>
public interface IInstrumentPanelProvider
{
    /// <summary>A stable id, e.g. "scpi".</summary>
    string Id { get; }

    /// <summary>The Device menu text, with an <c>_</c> before the access key, e.g. "_SCPI Instrument...".</summary>
    string MenuTitle { get; }

    /// <summary>The title of the instrument picker.</summary>
    string PickerTitle { get; }

    /// <summary>The structured presenter that correlates replies for the panel and feeds its indicators.</summary>
    string PresenterName { get; }

    /// <summary>The picker choice that asks the device which instrument it is.</summary>
    string AutoDetectChoice { get; }

    /// <summary>The picker choice for an instrument with no profile of its own.</summary>
    string GenericChoice { get; }

    /// <summary>The names of the instruments with a profile, in picker order (excluding the auto-detect and generic choices).</summary>
    IReadOnlyList<string> ProfileNames { get; }

    /// <summary>Whether the panel makes sense for a connection (the menu item is disabled otherwise). Called only while connected.</summary>
    bool IsAvailable(string transport, int vendorId, int productId);

    /// <summary>
    /// Applies what a named profile says about the wire (its reply terminator) to <paramref name="presenter"/> without opening a panel,
    /// so a scripted session of a terminatorless instrument does not wait for a line ending that never comes. False when no profile has that name.
    /// </summary>
    bool ConfigureForProfile(string profileName, IPresenter? presenter);

    /// <summary>Opens the panel for <paramref name="choice"/> (a profile name or <see cref="GenericChoice"/>).</summary>
    InstrumentPanel Open(string choice, Session session, IPresenter? structuredSource);

    /// <summary>The line shown while <see cref="DetectAsync"/> waits.</summary>
    string DetectProgressMessage(TimeSpan timeout);

    /// <summary>Asks the device which instrument it is. A send failure propagates (the session has then closed itself and reported why).</summary>
    Task<InstrumentDetection> DetectAsync(Session session, IPresenter? structuredSource, TimeSpan timeout);
}
