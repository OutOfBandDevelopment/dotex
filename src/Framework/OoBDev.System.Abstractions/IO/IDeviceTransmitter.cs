using System.Threading.Tasks;

namespace OoBDev.System.IO;

public interface IDeviceTransmitter<TMessage> : IDevice<TMessage>
{
    Task<bool> Transmit(TMessage message);
}
