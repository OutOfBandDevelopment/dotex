namespace OoBDev.System.IO.Devices;

/// <summary>Where a configuration editor reads and writes: a host/port (UDP or TCP, the editor decides) typed or taken from network discovery.</summary>
public sealed record DeviceConfigTarget(string Host, int Port = 0);
