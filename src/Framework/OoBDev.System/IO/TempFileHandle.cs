using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OoBDev.System.IO;

/// <summary>
/// Represents a handle for managing temporary files.
/// </summary>
public partial record TempFileHandle : ITempFile
{
    // https://github.com/mwwhited/BinaryDataDecoders/blob/6a31bae265d15ec3d61647453f50f49536a9391c/src/BinaryDataDecoders.ToolKit/IO/TempFileHandle.cs

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TempFileHandle"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging operations.</param>
    public TempFileHandle(
        ILogger<TempFileHandle>? logger = default
        ) => _logger = logger?? new ConsoleLogger<TempFileHandle>();

    /// <summary>
    /// Gets the path of the temporary file.
    /// </summary>
    public string FilePath { get; } = Path.GetTempFileName();

    /// <summary>
    /// Returns the path of the temporary file.
    /// </summary>
    /// <returns>The path of the temporary file.</returns>
    public override string ToString() => FilePath;

    /// <summary>
    /// Releases the resources used by the temporary file.
    /// </summary>
#pragma warning disable MA0055 // the finalizer deletes the temporary file when the handle is never disposed
    ~TempFileHandle() => Dispose(false);
#pragma warning restore MA0055

    /// <summary>
    /// Releases the resources used by the temporary file.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

#pragma warning disable IDE0060 // Remove unused parameter
    private void Dispose(bool disposing)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        if (!File.Exists(FilePath)) return;

        try
        {
            Log.LogDeleting(_logger, FilePath);
            File.Delete(FilePath);
        }
        catch
        {
            //Note: yeah, I don't care why it failed.
            if (!OperatingSystem.IsWindows())
                throw;

            try
            {
                Log.LogDeletingOnReboot(_logger, FilePath);
                NativeWin32Methods.MoveFileEx(FilePath, null, NativeWin32Methods.MoveFileFlags.DelayUntilReboot);
                //scheduled for reboot so good to go
                return;
            }
            catch
            {
                //yep, another.  it's just a temp file... give up, it doesn't work.  
            }

            //something happen above so throw the original exception.
            throw;
        }
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Deleting {FilePath}")]
        internal static partial void LogDeleting(ILogger logger, string filePath);

        [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Deleting {FilePath} (MoveFileEx)")]
        internal static partial void LogDeletingOnReboot(ILogger logger, string filePath);
    }
}
