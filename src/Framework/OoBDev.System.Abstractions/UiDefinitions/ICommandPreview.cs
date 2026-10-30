using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.UiDefinitions;

/// <summary>
/// Optional capability an <see cref="IControlSurface"/> can also implement: shows exactly what
/// <see cref="IControlSurface.InvokeAsync"/> would put on the wire for a given command id and value,
/// without sending anything or changing the surface's state. Both control-panel renderers use it for
/// a control's "sends" preview (a WPF info-icon tooltip, the TUI's footer line) — a separate
/// interface rather than a new <see cref="IControlSurface"/> member, so every existing surface (and
/// every test fake) keeps compiling unchanged and simply shows no preview.
/// </summary>
public interface ICommandPreview
{
    /// <summary>
    /// A human-readable rendering of what invoking <paramref name="commandId"/> with
    /// <paramref name="value"/> would send right now — e.g. <c>MEAS:VOLT:DC? DEF\n</c> for a text
    /// protocol (control characters escaped, see <see cref="CommandPreviewFormat.EscapeControlCharacters"/>)
    /// or hex report bytes for a binary one (see <see cref="CommandPreviewFormat.ToHex"/>). Null when
    /// that invocation sends nothing (a value-holder field, a state-only setter applied later) or the
    /// command id isn't one this surface knows. Never throws for a bad value; never sends.
    /// </summary>
    string? PreviewCommand(string commandId, string? value);
}
