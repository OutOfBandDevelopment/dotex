using System.Runtime.InteropServices;
using System.Security;

namespace OoBDev.System;

/// <summary>
/// Provides extension methods for SecureString operations.
/// </summary>
public static class SecureStringEx
{
    /// <summary>
    /// Converts a SecureString to a plain text string.
    /// </summary>
    /// <param name="secure">The secure string to convert.</param>
    /// <returns>The plain text string, or null if the input is null.</returns>
    /// <remarks>
    /// WARNING: This method defeats the purpose of SecureString by exposing the protected data.
    /// Use only when absolutely necessary and ensure the returned string is properly disposed.
    /// </remarks>
    public static string? GetUnsecureString(this SecureString? secure)
    {
        // http://blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-string.aspx 
        if (secure == null)
            return null;

        var unmanagedString = nint.Zero;
        try
        {
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secure);
            return Marshal.PtrToStringUni(unmanagedString);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
        }
    }
}
