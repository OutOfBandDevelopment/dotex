namespace OoBDev.Kuando.Busylight;

/// <summary>
/// The device's answer to a poll: an ASCII identification string (for example <c>PLENOM</c> followed by serial and date fields).
/// </summary>
/// <param name="Text">The report with every non-printable byte shown as a dot.</param>
public readonly record struct BusylightIdentification(string Text);
