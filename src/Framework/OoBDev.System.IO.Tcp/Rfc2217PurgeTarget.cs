namespace OoBDev.System.IO.Tcp;

/// <summary>RFC 2217 PURGE-DATA wire values.</summary>
internal enum Rfc2217PurgeTarget : byte
{
    ReceiveBuffer = 1,
    TransmitBuffer = 2,
    Both = 3,
}
