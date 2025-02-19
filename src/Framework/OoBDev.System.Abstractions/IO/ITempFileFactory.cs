namespace OoBDev.System.IO;

/// <summary>
/// this is a provider for managed temp files
/// </summary>
public interface ITempFileFactory
{
    /// <summary>
    /// Get a managed temporary file. ITempFile will be deleted when disposed.
    /// </summary>
    /// <returns></returns>
    ITempFile GetTempFile();
}
