using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Media.Metadata.Raw;

/// <summary>Detects camera RAW files by content and develops them to an ordinary image. The source stream is only read.</summary>
public interface IRawDeveloper
{
    /// <summary>Whether the stream holds a camera RAW file; a seekable stream is returned to its start position.</summary>
    /// <param name="input">The file content.</param>
    bool IsRaw(Stream input);

    /// <summary>Develops the RAW file and writes the image to <paramref name="output"/>.</summary>
    /// <param name="input">The RAW file; never changed.</param>
    /// <param name="output">Where the image is written.</param>
    /// <param name="options">The development options.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <exception cref="System.NotSupportedException">The developer cannot honour <paramref name="options"/>.</exception>
    /// <exception cref="System.IO.InvalidDataException">The input is not a RAW file or holds nothing to develop.</exception>
    Task DevelopAsync(Stream input, Stream output, RawDevelopOptions options, CancellationToken cancellationToken);
}
