﻿using System.Runtime.InteropServices;
using System.Security;

namespace OoBDev.System;

public static class SecureStringEx
{
    public static string? GetUnsecureString(this SecureString? secure)
    {
        // http://blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-string.aspx 
        if (secure == null)
            return null;

        nint unmanagedString = nint.Zero;
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
