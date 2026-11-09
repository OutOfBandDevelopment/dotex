namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>An open serial connection. Disposing must be idempotent and unblock pending reads.</summary>
public interface ISerialLink : ISerialControl, IDisposable
{
    public Stream Stream { get; }
}
