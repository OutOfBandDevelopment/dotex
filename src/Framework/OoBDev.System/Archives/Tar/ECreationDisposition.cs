namespace OoBDev.System.Archives.Tar;

/// <summary>
/// Specifies the action to take when creating or opening a file (Windows API CreateFile).
/// </summary>
public enum ECreationDisposition : uint
{
    /// <summary>
    /// Creates a new file, only if it does not already exist.
    /// </summary>
    New = 1,

    /// <summary>
    /// Creates a new file, always. If the file exists, it is overwritten.
    /// </summary>
    CreateAlways = 2,

    /// <summary>
    /// Opens a file, only if it exists.
    /// </summary>
    OpenExisting = 3,

    /// <summary>
    /// Opens a file, always. If the file does not exist, it is created.
    /// </summary>
    OpenAlways = 4,

    /// <summary>
    /// Opens a file and truncates it so that its size is zero bytes, only if it exists.
    /// </summary>
    TruncateExisting = 5,
}
