using System.IO;
using System.Threading.Tasks;

namespace OoBDev.System.IO;

/// <summary>
/// Provides extension methods for Stream objects, including temporary file creation.
/// </summary>
public static class StreamEx
{
    /// <summary>
    /// Asynchronously copies the stream contents to a temporary file.
    /// The temporary file is automatically cleaned up when the returned ITempFile is disposed.
    /// </summary>
    /// <param name="stream">The stream to copy to a temporary file.</param>
    /// <returns>An ITempFile handle representing the temporary file containing the stream data.</returns>
    public static async Task<ITempFile> AsTempFileAsync(this Stream stream)
    {
        var temp = new TempFileHandle();
        using var fs = File.OpenWrite(temp.FilePath);
        await stream.CopyToAsync(fs).ConfigureAwait(false);
        fs.Close();
        return temp;
    }

    /// <summary>
    /// Synchronously copies the stream contents to a temporary file.
    /// The temporary file is automatically cleaned up when the returned ITempFile is disposed.
    /// </summary>
    /// <param name="stream">The stream to copy to a temporary file.</param>
    /// <returns>An ITempFile handle representing the temporary file containing the stream data.</returns>
    public static ITempFile AsTempFile(this Stream stream)
    {
        var temp = new TempFileHandle();
        using var fs = File.OpenWrite(temp.FilePath);
        stream.CopyTo(fs);
        fs.Close();
        return temp;
    }
}
