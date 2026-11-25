namespace OoBDev.System.IO.Brokers;

public sealed class AmqpConnectionFactory : IBrokerConnectionFactory
{
    public IBrokerConnection Create() => new AmqpConnection();
}
