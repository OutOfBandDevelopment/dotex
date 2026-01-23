using System.Runtime.InteropServices;

namespace OoBDev.Archives.Tar;

[StructLayout(LayoutKind.Sequential)]
internal struct FILETIME
{
    internal uint dwLowDateTime;
    internal uint dwHighDateTime;
};
