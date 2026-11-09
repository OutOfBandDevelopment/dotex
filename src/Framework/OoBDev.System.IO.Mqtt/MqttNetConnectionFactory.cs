namespace OoBDev.System.IO.Mqtt;

public sealed class MqttNetConnectionFactory : IMqttConnectionFactory
{
    public IMqttConnection Create() => new MqttNetConnection();
}
