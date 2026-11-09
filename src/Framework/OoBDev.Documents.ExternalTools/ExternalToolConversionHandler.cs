using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OoBDev.Documents.Conversion;
using OoBDev.System.IO;

namespace OoBDev.Documents.ExternalTools;

/// <summary>Converts a document by running one registered external tool (<see cref="ExternalToolOptions"/>) over temporary files.</summary>
/// <remarks>
/// The template is split on whitespace and the placeholders are substituted inside each token before it joins
/// <see cref="ProcessStartInfo.ArgumentList"/>, so a path with spaces stays one argument and there is no shell and no quoting step.
/// </remarks>
public sealed class ExternalToolConversionHandler : IDocumentConversionHandler
{
    private readonly ExternalToolOptions _tool;
    private readonly ITempFileFactory _temp;

    /// <summary>Creates a handler for one tool.</summary>
    /// <param name="tool">The registered tool.</param>
    /// <param name="temp">Creates the temporary folder the tool reads and writes in.</param>
    public ExternalToolConversionHandler(ExternalToolOptions tool, ITempFileFactory temp)
    {
        ArgumentNullException.ThrowIfNull(tool);
        ArgumentNullException.ThrowIfNull(temp);
        _tool = tool;
        _temp = temp;
        Sources = ContentTypeNames.Resolve(tool.Formats);
        Destinations = ContentTypeNames.Resolve(tool.OutputExtension);
    }

    /// <summary>The registered tool's name.</summary>
    public string Name => _tool.Name;

    /// <inheritdoc />
    /// <remarks>Empty when the tool accepts any format.</remarks>
    public string[] Sources { get; }

    /// <inheritdoc />
    public string[] Destinations { get; }

    /// <inheritdoc />
    public bool SupportedSource(string contentType) =>
        Sources.Length == 0 || Sources.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public bool SupportedDestination(string contentType) =>
        Destinations.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">A content type is not supported.</exception>
    /// <exception cref="InvalidOperationException">The tool is not configured, cannot start, exits non-zero or writes no output.</exception>
    public async Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        if (!SupportedSource(sourceContentType)) throw new NotSupportedException($"Source content type \"{sourceContentType}\" is not supported");
        if (!SupportedDestination(destinationContentType)) throw new NotSupportedException($"Destination content type \"{destinationContentType}\" is not supported");
        if (string.IsNullOrWhiteSpace(_tool.Path)) throw new InvalidOperationException($"No path is configured for the tool '{_tool.Name}'.");

        using var folder = _temp.GetTempDirectory();
        var inputPath = Path.Combine(folder.DirectoryPath, "input." + ContentTypeNames.ExtensionFor(sourceContentType));
        var outputPath = Path.Combine(folder.DirectoryPath, "output." + ContentTypeNames.ExtensionFor(destinationContentType));
        var input = File.Create(inputPath);
        await using (input.ConfigureAwait(false))
        {
            await source.CopyToAsync(input).ConfigureAwait(false);
        }

        await RunAsync(inputPath, outputPath).ConfigureAwait(false);

        var output = File.OpenRead(outputPath);
        await using (output.ConfigureAwait(false))
        {
            await output.CopyToAsync(destination).ConfigureAwait(false);
        }

        await destination.FlushAsync().ConfigureAwait(false);
    }

    private async Task RunAsync(string inputPath, string outputPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _tool.Path!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        foreach (var rawToken in _tool.Arguments.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            startInfo.ArgumentList.Add(rawToken
                .Replace("{input}", inputPath, StringComparison.Ordinal)
                .Replace("{output}", outputPath, StringComparison.Ordinal)
                .Replace("{dpi}", _tool.Dpi.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        }

        try
        {
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Could not start '{_tool.Path}'.");
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync().ConfigureAwait(false);
            await stdout.ConfigureAwait(false);
            var error = await stderr.ConfigureAwait(false);
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"'{_tool.Path}' exited with code {process.ExitCode}: {error.Trim()}");
            }
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }

        if (!File.Exists(outputPath))
        {
            throw new InvalidOperationException($"'{_tool.Path}' exited successfully but did not produce an output file.");
        }
    }
}
