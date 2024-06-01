using System;

namespace OoBDev.System.IO;

public interface IDeviceReceiver<TMessage> : IDevice<TMessage>
{
    event EventHandler<TMessage> MessageReceived;
}
