using System.Runtime.InteropServices;

namespace OoBDev.System.Archives.Tar;

[StructLayout(LayoutKind.Sequential)]
internal struct FILETIME
{
    internal uint dwLowDateTime;
    internal uint dwHighDateTime;
};
