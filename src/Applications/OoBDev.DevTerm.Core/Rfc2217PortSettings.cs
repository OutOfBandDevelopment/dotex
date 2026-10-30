using OoBDev.System.IO.Pipelines;
using OoBDev.System.IO;
using OoBDev.System.IO.Tcp;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OoBDev.DevTerm;

/// <summary>The serial settings a remote RFC 2217 client has asked for.</summary>
public sealed record Rfc2217PortSettings(int BaudRate = 9600, int DataBits = 8, Parity Parity = Parity.None, StopBits StopBits = StopBits.One, bool Dtr = true, bool Rts = true);
