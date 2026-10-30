using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace OoBDev.System.IO.Brokers;

public sealed class AmqpConnectionFactory : IBrokerConnectionFactory
{
    public IBrokerConnection Create() => new AmqpConnection();
}
