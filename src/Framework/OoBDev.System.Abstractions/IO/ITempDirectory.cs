using System;

namespace OoBDev.System.IO;

/// <summary>
/// Temp directory handle; the directory and everything in it is deleted when the handle is disposed.
/// </summary>
public interface ITempDirectory : IDisposable
{
    /// <summary>
    /// Path to the temp directory (it exists until disposal).
    /// </summary>
    string DirectoryPath { get; }
}
