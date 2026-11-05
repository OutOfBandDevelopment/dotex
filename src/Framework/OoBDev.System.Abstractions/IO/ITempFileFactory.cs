namespace OoBDev.System.IO;

/// <summary>
/// this is a provider for managed temp files and directories
/// </summary>
public interface ITempFileFactory
{
    /// <summary>
    /// Get a managed temporary file. ITempFile will be deleted when disposed.
    /// </summary>
    /// <returns></returns>
    ITempFile GetTempFile();

    /// <summary>
    /// Get a managed temporary directory. The directory and its contents are deleted when disposed.
    /// </summary>
    ITempDirectory GetTempDirectory();
}
