namespace OoBDev.System.IO.Tcp;

/// <summary>Server-pushed UART modem-status change. Only ever server-originated, so <see cref="IsFromServer"/> is always <c>true</c>.</summary>
internal sealed record Rfc2217NotifyModemStateMessage(Rfc2217ModemState ModemState) : IRfc2217Message
{
    public bool IsFromServer => true;
}
