using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OoBDev.DevTerm;
using OoBDev.UiDefinitions;
namespace OoBDev.DevTerm.Web.Components;

/// <summary>
/// The control panel the host serves: its definition plus the surface that carries a control's command to the device.
/// Registered empty before the host is built (the session it drives only exists afterwards), then filled by <see cref="Set"/>.
/// </summary>
public sealed class PanelHostHolder
{
    public UiDefinition? Definition { get; private set; }

    public IControlSurface? Surface { get; private set; }

    private readonly global::System.Collections.Concurrent.ConcurrentDictionary<string, string> _values = new(StringComparer.Ordinal);

    /// <summary>The latest published value per indicator id (what a freshly opened page starts from).</summary>
    public IReadOnlyDictionary<string, string> Values => _values;

    /// <summary>Raised after a batch of published values was merged into <see cref="Values"/>.</summary>
    public event Action? ValuesChanged;

    /// <summary>Merges a structured presenter's <c>ValuesChanged</c> batch and tells every open panel to re-render.</summary>
    public void Publish(IReadOnlyDictionary<string, string> values)
    {
        foreach (var (id, value) in values)
        {
            _values[id] = value;
        }

        ValuesChanged?.Invoke();
    }

    public void Set(UiDefinition definition, IControlSurface surface)
    {
        Definition = definition;
        Surface = surface;
    }
}
