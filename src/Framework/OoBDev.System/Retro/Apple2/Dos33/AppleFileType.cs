using System;

namespace OoBDev.System.Retro.Apple2.Dos33;


/// <summary>
/// Represents the file types used in Apple II DOS 3.3.
/// </summary>
[Flags]
public enum AppleFileType : byte
{
    /// <summary>
    /// Text file.
    /// </summary>
    Text = 0x00,

    /// <summary>
    /// Integer BASIC program.
    /// </summary>
    IntegerBasic = 0x01,

    /// <summary>
    /// Applesoft BASIC program.
    /// </summary>
    ApplesoftBasic = 0x02,

    /// <summary>
    /// Binary file.
    /// </summary>
    Binary = 0x04,

    /// <summary>
    /// S-type file (specific use unknown).
    /// </summary>
    SType = 0x08,

    /// <summary>
    /// Relocatable object module.
    /// </summary>
    RelocatableObjectModule = 0x10,

    /// <summary>
    /// A-type file (specific use unknown).
    /// </summary>
    AType = 0x20,

    /// <summary>
    /// B-type file (specific use unknown).
    /// </summary>
    BType = 0x40,

    /// <summary>
    /// Indicates that the file is locked (read-only).
    /// </summary>
    Locked = 0x80,
}
