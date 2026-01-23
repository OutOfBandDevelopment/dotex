namespace OoBDev.System.Archives.Tar;

/// <summary>
/// Specifies the type of file entry in a TAR archive.
/// </summary>
public enum TarFileType : byte
{
    /// <summary>
    /// Regular file (type indicator '0').
    /// </summary>
    File = (byte)'0',

    /// <summary>
    /// Regular file using the old TAR format (type indicator '\0').
    /// </summary>
    FileOld = (byte)'\0',

    /// <summary>
    /// Hard link to another file (type indicator '1').
    /// </summary>
    HardLink = (byte)'1',

    /// <summary>
    /// Symbolic link to another file (type indicator '2').
    /// </summary>
    SymbolicLink = (byte)'2',

    /// <summary>
    /// Character special device (type indicator '3').
    /// </summary>
    CharacterDevice = (byte)'3',

    /// <summary>
    /// Block special device (type indicator '4').
    /// </summary>
    BlockDevice = (byte)'4',

    /// <summary>
    /// Directory entry (type indicator '5').
    /// </summary>
    Directory = (byte)'5',

    /// <summary>
    /// Named pipe (FIFO) (type indicator '6').
    /// </summary>
    NamedPipe = (byte)'6',

    /// <summary>
    /// Contiguous file (type indicator '7').
    /// </summary>
    ContiguousFile = (byte)'7',

    /// <summary>
    /// GNU tar extension for long symbolic link names (type indicator 'K').
    /// </summary>
    LongSymbolicLink = (byte)'K',

    /// <summary>
    /// GNU tar extension for long file names (type indicator 'L').
    /// </summary>
    LongName = (byte)'L',

    /// <summary>
    /// Sparse file (type indicator 'S').
    /// </summary>
    SparseFile = (byte)'S',

    /// <summary>
    /// Volume header (type indicator 'V').
    /// </summary>
    Volume = (byte)'V',
}
