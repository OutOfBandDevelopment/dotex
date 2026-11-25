using System;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Web.Tests;

/// <summary>Temp folders and files for helpers that hand a path to a web host and outlive a single test; disposed once at the end of the run.</summary>
[TestClass]
public static class WebTestTemp
{
    private static readonly ConcurrentBag<IDisposable> _handles = [];

    /// <summary>A new, empty temp folder.</summary>
    public static string Directory()
    {
        var handle = new TempDirectoryHandle();
        _handles.Add(handle);
        return handle.DirectoryPath;
    }

    /// <summary>A path for a file inside a new temp folder.</summary>
    public static string File(string name) => global::System.IO.Path.Combine(Directory(), name);

    [AssemblyCleanup]
    public static void DeleteAll()
    {
        while (_handles.TryTake(out var handle))
        {
            handle.Dispose();
        }
    }
}
