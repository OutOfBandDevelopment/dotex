namespace OoBDev.DevTerm.Devices.Scpi;

/// <summary>
/// Line-buffers SCPI replies and matches each complete line to the oldest pending query. One instance is shared across the
/// open SCPI panel's profiles, so an instrument whose replies carry no terminator (Korad) is handled by
/// <see cref="LineReplyPresenter.ConfigureTerminator"/>. CR, LF and CRLF each end a line (a Tektronix TDS2024 ends with a bare CR).
/// </summary>
public sealed class ScpiReplyPresenter : LineReplyPresenter
{
    /// <inheritdoc />
    public override string Name => "scpi";
}
