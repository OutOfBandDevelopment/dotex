using System.Runtime.InteropServices;

namespace OoBDev.System.Archives.Tar;

/// <summary>
/// Represents security attributes for Windows API calls (SECURITY_ATTRIBUTES structure).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SECURITY_ATTRIBUTES
{
    /// <summary>
    /// The size, in bytes, of this structure.
    /// </summary>
    public int nLength;

    /// <summary>
    /// A pointer to a SECURITY_DESCRIPTOR structure that controls access to the object.
    /// </summary>
    public nint lpSecurityDescriptor;

    /// <summary>
    /// Specifies whether the returned handle is inherited when a new process is created.
    /// </summary>
    public int bInheritHandle;
}
