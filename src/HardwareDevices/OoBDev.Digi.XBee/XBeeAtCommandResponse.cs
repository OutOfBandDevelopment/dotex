namespace OoBDev.Digi.XBee;

/// <summary>The local radio answering an AT command.</summary>
/// <param name="FrameId">The frame id of the request this answers.</param>
/// <param name="Command">The two-letter command.</param>
/// <param name="Status">0 OK, 1 error, 2 invalid command, 3 invalid parameter, 4 transmit failure.</param>
/// <param name="Value">The returned parameter bytes, empty when the command returns none.</param>
public sealed record XBeeAtCommandResponse(byte FrameId, string Command, byte Status, byte[] Value) : XBeeFrame(XBeeFrameTypes.AtCommandResponse)
{
    /// <summary>True when <see cref="Status"/> is 0.</summary>
    public bool IsOk => Status == 0;
}
