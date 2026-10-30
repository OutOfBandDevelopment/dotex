using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

public enum RoutingState
{
    Stopped,
    Connecting,
    Connected,

    /// <summary>The broker is unreachable or was lost; retrying with backoff. <see cref="RoutingService.Reason"/> says why.</summary>
    Reconnecting,

    /// <summary>The configuration is invalid; routing did not start. <see cref="RoutingService.Reason"/> says why.</summary>
    Failed,
}
