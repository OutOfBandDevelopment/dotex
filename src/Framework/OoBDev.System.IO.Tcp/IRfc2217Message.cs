namespace OoBDev.System.IO.Tcp;

/// <summary>A decoded COM-PORT-OPTION subnegotiation message. <see cref="IsFromServer"/> reflects whether the wire command code carried <see cref="Rfc2217Command.ServerOffset"/>.</summary>
internal interface IRfc2217Message
{
    bool IsFromServer { get; }
}
