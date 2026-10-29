using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>
/// A device's own configuration (a network bridge's IP, work mode, serial settings) edited from dev-term instead of the vendor's tool
/// (docs/design/proposals/network-device-config-editors.md). The form is a generic <see cref="UiDefinition"/>; values travel as
/// strings keyed by control id so an editor never needs a front-end-specific settings type. Independent of any open session.
/// </summary>
public interface IDeviceConfigEditor
{
    /// <summary>A stable id, e.g. "ebyte-e810".</summary>
    string Id { get; }

    /// <summary>The Device, Configure device menu text.</summary>
    string Title { get; }

    /// <summary>The form the editor shows.</summary>
    UiDefinition BuildDefinition();

    /// <summary>Reads the device's current settings, keyed by control id.</summary>
    Task<IReadOnlyDictionary<string, string>> ReadAsync(DeviceConfigTarget target, CancellationToken cancellationToken);

    /// <summary>Checks edited values before any write; an empty list means they can be written.</summary>
    IReadOnlyList<DeviceConfigIssue> Validate(IReadOnlyDictionary<string, string> values);

    /// <summary>
    /// Whether writing <paramref name="values"/> over <paramref name="current"/> will drop a connection to <paramref name="target"/>
    /// (a changed IP or port), so the front end can warn first.
    /// </summary>
    bool WillDropConnection(DeviceConfigTarget target, IReadOnlyDictionary<string, string> current, IReadOnlyDictionary<string, string> values);

    /// <summary>Writes the values, then reads them back.</summary>
    Task<DeviceConfigWriteResult> WriteAsync(DeviceConfigTarget target, IReadOnlyDictionary<string, string> values, CancellationToken cancellationToken);
}
