using System;
using System.Threading.Tasks;

namespace OoBDev.System.IO;

public interface IStreamDevice<TMessage> : IDeviceReceiver<TMessage>, IDeviceTransmitter<TMessage>, IDisposable
{
    Task Runner { get; }

    new event EventHandler<TMessage> MessageReceived;
    event EventHandler<StreamDeviceStatus> DeviceStatus;
    event EventHandler<DeviceErrorEventArgs> MessageReceivedError;
    event EventHandler<DeviceErrorEventArgs> MessageTrasmitterError;

    new Task<bool> Transmit(TMessage message);
}