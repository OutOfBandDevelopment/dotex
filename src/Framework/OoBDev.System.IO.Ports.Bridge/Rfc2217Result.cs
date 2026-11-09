namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>Outcome of feeding client bytes to an <see cref="Rfc2217Session"/>.</summary>
/// <param name="SerialData">Payload bytes (telnet framing removed) to write to the serial port.</param>
/// <param name="Reply">Negotiation and COM-PORT replies to send back to the client.</param>
public readonly record struct Rfc2217Result(byte[] SerialData, byte[] Reply);
