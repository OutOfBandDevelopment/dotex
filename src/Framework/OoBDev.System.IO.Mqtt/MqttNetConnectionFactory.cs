using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;
using MQTTnet;
using MQTTnet.Protocol;

namespace OoBDev.System.IO.Mqtt;

public sealed class MqttNetConnectionFactory : IMqttConnectionFactory
{
    public IMqttConnection Create() => new MqttNetConnection();
}
