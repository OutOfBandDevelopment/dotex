using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>RFC 2217 PURGE-DATA wire values.</summary>
internal enum Rfc2217PurgeTarget : byte
{
    ReceiveBuffer = 1,
    TransmitBuffer = 2,
    Both = 3,
}
