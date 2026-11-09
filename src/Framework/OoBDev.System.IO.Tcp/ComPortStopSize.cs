namespace OoBDev.System.IO.Tcp;

/// <summary>RFC 2217 SET-STOPSIZE wire values. <see cref="Request"/> asks the server to report the current value.</summary>
internal enum ComPortStopSize : byte
{
    Request = 0,
    One = 1,
    Two = 2,
    OneAndAHalf = 3,
}
