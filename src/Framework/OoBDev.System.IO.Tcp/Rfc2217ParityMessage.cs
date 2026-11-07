using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217ParityMessage(ComPortParity Parity, bool IsFromServer) : IRfc2217Message;
