using System;
using System.Collections.Generic;

namespace OoBDev.DevTerm;

/// <summary>An out-of-process plugin that was found but not yet approved: what to ask, and how to start it once approved.</summary>
public sealed record PendingPlugin(PluginApprovalRequest Request, string Command, IReadOnlyList<string> Arguments, TimeSpan ReplyTimeout);
