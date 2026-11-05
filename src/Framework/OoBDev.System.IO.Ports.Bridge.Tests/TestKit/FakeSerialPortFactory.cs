using System.Collections.Concurrent;

namespace OoBDev.System.IO.Ports.Bridge.Tests.TestKit;

public sealed class FakeSerialPortFactory : ISerialPortFactory
{
    private int _failuresRemaining;

    public FakeSerialPortFactory(int failFirstOpens = 0)
    {
        _failuresRemaining = failFirstOpens;
    }

    public ConcurrentQueue<FakeSerialLink> Links { get; } = new();

    public int OpenAttempts { get; private set; }

    public ISerialLink Open(BridgeOptions options)
    {
        OpenAttempts++;
        if (_failuresRemaining > 0)
        {
            _failuresRemaining--;
            throw new IOException("Port not present.");
        }

        var link = new FakeSerialLink();
        Links.Enqueue(link);
        return link;
    }

    public FakeSerialLink? Latest => Links.LastOrDefault();
}
