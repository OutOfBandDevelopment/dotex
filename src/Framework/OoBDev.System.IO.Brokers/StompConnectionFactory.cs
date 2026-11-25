namespace OoBDev.System.IO.Brokers;

public sealed class StompConnectionFactory : IBrokerConnectionFactory
{
    public IBrokerConnection Create() => new StompConnection();
}
