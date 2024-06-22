namespace OoBDev.System.IO;

/// <summary>
/// Enumeration based on ASCII control characters  
/// </summary>
/// <remarks>
/// https://en.wikipedia.org/wiki/ASCII http://www.asciitable.com/
/// </remarks>
public enum ControlCharacters : byte
{
    /// <summary>
    /// Null character (0x00).
    /// </summary>
    Null = 0x00,
    /// <summary>
    /// Start of Heading (0x01).
    /// </summary>
    StartOfHeading = 0x01,
    /// <summary>
    /// Start of Text (0x02).
    /// </summary>
    StartOfText = 0x02,
    /// <summary>
    /// End of Text (0x03).
    /// </summary>
    EndOfText = 0x03,
    /// <summary>
    /// End of Transmission (0x04).
    /// </summary>
    EndOfTransmission = 0x04,
    /// <summary>
    /// Enquiry (0x05).
    /// </summary>
    Enquiry = 0x05,
    /// <summary>
    /// Acknowledgment (0x06).
    /// </summary>
    Acknowledgment = 0x06,
    /// <summary>
    /// Bell (0x07).
    /// </summary>
    Bell = 0x07,
    /// <summary>
    /// Backspace (0x08).
    /// </summary>
    BackSpace = 0x08,
    /// <summary>
    /// Horizontal Tab (0x09).
    /// </summary>
    HorizontalTab = 0x09,
    /// <summary>
    /// Line Feed (0x0A).
    /// </summary>
    LineFeed = 0x0A,
    /// <summary>
    /// Vertical Tab (0x0B).
    /// </summary>
    VerticalTab = 0x0B,
    /// <summary>
    /// Form Feed (0x0C).
    /// </summary>
    FormFeed = 0x0C,
    /// <summary>
    /// Carriage Return (0x0D).
    /// </summary>
    CarriageReturn = 0x0D,
    /// <summary>
    /// Shift Out (0x0E).
    /// </summary>
    ShiftOut = 0x0E,
    /// <summary>
    /// Shift In (0x0F).
    /// </summary>
    ShiftIn = 0x0F,
    /// <summary>
    /// Data Line Escape (0x10).
    /// </summary>
    DataLineEscape = 0x10,
    /// <summary>
    /// Device Control 1 (0x11).
    /// </summary>
    DeviceControl1 = 0x11,
    /// <summary>
    /// Device Control 2 (0x12).
    /// </summary>
    DeviceControl2 = 0x12,
    /// <summary>
    /// Device Control 3 (0x13).
    /// </summary>
    DeviceControl3 = 0x13,
    /// <summary>
    /// Device Control 4 (0x14).
    /// </summary>
    DeviceControl4 = 0x14,
    /// <summary>
    /// Negative Acknowledgement (0x15).
    /// </summary>
    NegativeAcknowledgement = 0x15,
    /// <summary>
    /// Synchronous Idle (0x16).
    /// </summary>
    SynchronousIdle = 0x16,
    /// <summary>
    /// End of Transmit Block (0x17).
    /// </summary>
    EndOfTransmitBlock = 0x17,
    /// <summary>
    /// Cancel (0x18).
    /// </summary>
    Cancel = 0x18,
    /// <summary>
    /// End of Medium (0x19).
    /// </summary>
    EndOfMedium = 0x19,
    /// <summary>
    /// Substitute (0x1A).
    /// </summary>
    Substitute = 0x1A,
    /// <summary>
    /// Escape (0x1B).
    /// </summary>
    Escape = 0x1B,
    /// <summary>
    /// File Separator (0x1C).
    /// </summary>
    FileSeparator = 0x1C,
    /// <summary>
    /// Group Separator (0x1D).
    /// </summary>
    GroupSeparator = 0x1D,
    /// <summary>
    /// Record Separator (0x1E).
    /// </summary>
    RecordSeparator = 0x1E,
    /// <summary>
    /// Unit Separator (0x1F).
    /// </summary>
    UnitSeparator = 0x1F,

    /// <summary>
    /// Space (0x20).
    /// </summary>
    Space = 0x20,
    /// <summary>
    /// Delete (0x7F).
    /// </summary>
    Delete = 0x7F,
}

