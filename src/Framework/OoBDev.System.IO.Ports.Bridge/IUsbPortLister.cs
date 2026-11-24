namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>Lists the serial ports present now with their USB ids.</summary>
public interface IUsbPortLister
{
    public IReadOnlyList<UsbPortInfo> List();
}
