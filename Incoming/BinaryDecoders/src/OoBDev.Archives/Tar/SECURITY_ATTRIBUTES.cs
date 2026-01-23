using System;
using System.Runtime.InteropServices;

namespace OoBDev.Archives.Tar;

[StructLayout(LayoutKind.Sequential)]
public struct SECURITY_ATTRIBUTES
{
    public int nLength;
    public IntPtr lpSecurityDescriptor;
    public int bInheritHandle;
}
