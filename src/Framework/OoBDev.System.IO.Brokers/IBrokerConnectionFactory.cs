using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.System.IO.Brokers;

public interface IBrokerConnectionFactory
{
    IBrokerConnection Create();
}
