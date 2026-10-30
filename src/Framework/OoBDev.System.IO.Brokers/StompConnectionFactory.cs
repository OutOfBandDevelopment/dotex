using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
namespace OoBDev.System.IO.Brokers;

public sealed class StompConnectionFactory : IBrokerConnectionFactory
{
    public IBrokerConnection Create() => new StompConnection();
}
