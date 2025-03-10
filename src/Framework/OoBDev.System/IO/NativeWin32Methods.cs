using System;
using System.Runtime.InteropServices;

namespace OoBDev.System.IO;

internal class NativeWin32Methods
{
    // https://github.com/mwwhited/BinaryDataDecoders/blob/6a31bae265d15ec3d61647453f50f49536a9391c/src/BinaryDataDecoders.ToolKit/IO/NativeWin32Methods.cs

    // https://stackoverflow.com/questions/6077869/movefile-function-in-c-sharp-delete-file-after-reboot
    // https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-movefileexa?redirectedfrom=MSDN

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool MoveFileEx(string lpExistingFileName, string? lpNewFileName, MoveFileFlags dwFlags);

    [Flags]
    internal enum MoveFileFlags
    {
        None = 0,
        ReplaceExisting = 1,
        CopyAllowed = 2,
        DelayUntilReboot = 4,
        WriteThrough = 8,
        CreateHardlink = 16,
        FailIfNotTrackable = 32,
    }
}
