namespace OoBDev.System.IO.Tcp;

/// <summary>RFC 2217 SET-DATASIZE wire values. <see cref="Request"/> asks the server to report the current value.</summary>
internal enum ComPortDataSize : byte
{
    Request = 0,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
}
