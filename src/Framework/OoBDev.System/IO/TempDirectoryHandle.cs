using System;
using System.IO;

namespace OoBDev.System.IO;

/// <summary>
/// A uniquely named directory under the system temp folder, deleted with its contents on disposal.
/// </summary>
public sealed class TempDirectoryHandle : ITempDirectory
{
    /// <summary>
    /// Creates the directory.
    /// </summary>
    public TempDirectoryHandle()
    {
        DirectoryPath = Path.Combine(Path.GetTempPath(), $"oobdev_{Guid.NewGuid():N}");
        Directory.CreateDirectory(DirectoryPath);
    }

    /// <inheritdoc />
    public string DirectoryPath { get; }

    /// <summary>
    /// Returns the path of the temporary directory.
    /// </summary>
    public override string ToString() => DirectoryPath;

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
        catch (IOException)
        {
            // A file still held open by a process under test: the OS temp cleanup removes it later.
        }
        catch (UnauthorizedAccessException)
        {
            // Same: a read-only or locked file is not worth failing a test over.
        }
    }
}
