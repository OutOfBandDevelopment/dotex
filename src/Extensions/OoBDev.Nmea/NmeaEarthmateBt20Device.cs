using OoBDev.System.IO.UsbHids;

namespace OoBDev.Nmea;

/// <summary>
/// The DeLorme Earthmate GPS BT-20 (vendor 0x1163, product 0x0200), a 12 channel NMEA 2.0 receiver on USB HID. How its HID reports
/// are framed is not confirmed on hardware; <see cref="NmeaMessageDecoder"/> drops NUL padding, so any report length works.
/// </summary>
[UsbHid(0x1163, 0x0200)]
public sealed class NmeaEarthmateBt20Device : NmeaDevice;
