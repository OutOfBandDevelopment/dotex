using Microsoft.Extensions.Logging;
using OoBDev.AI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.LlmCodeGen.Cli;

/// <summary>
/// Walks the input folders, renders one prompt per folder, asks the model and saves the response and the files in it.
/// </summary>
/// <param name="completion">The selected model provider.</param>
/// <param name="log">Diagnostic output.</param>
/// <param name="time">The clock for the metadata file; the system clock when omitted.</param>
public partial class FolderPromptRunner(IMessageCompletion completion, ILogger<FolderPromptRunner> log, TimeProvider? time = null)
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    private readonly TimeProvider _time = time ?? TimeProvider.System;

    /// <summary>
    /// Runs every folder under the input path.
    /// </summary>
    /// <param name="options">The tool options.</param>
    /// <param name="cancellationToken">Stops the run between folders.</param>
    /// <returns>The number of folders sent to the model.</returns>
    public async Task<int> RunAsync(LlmCodeGenOptions options, CancellationToken cancellationToken)
    {
        var input = Path.GetFullPath(options.InputPath!);
        var output = Path.GetFullPath(options.OutputPath!);
        var (name, source) = PromptRenderer.Load(options.Template!);
        var sent = 0;

        var directories = new[] { input }
            .Concat(Directory.EnumerateDirectories(input, "*", SearchOption.AllDirectories))
            .Order(StringComparer.Ordinal);

        foreach (var directory in directories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var files = Directory.EnumerateFiles(directory, options.SearchPattern, SearchOption.TopDirectoryOnly)
                .Order(StringComparer.Ordinal)
                .ToList();
            if (files.Count == 0)
            {
                continue;
            }

            var relative = Path.GetRelativePath(input, directory);
            var target = relative == "." ? output : Path.GetFullPath(Path.Combine(output, relative));
            var responseFile = Path.Combine(target, name + ".response.md");
            if (File.Exists(responseFile) && !options.Overwrite)
            {
                LogSkipExisting(log, directory);
                continue;
            }

            var model = new
            {
                folder = relative == "." ? Path.GetFileName(input) : relative.Replace('\\', '/'),
                files = files.Select(f => new
                {
                    name = Path.GetFileName(f),
                    extension = Path.GetExtension(f),
                    content = File.ReadAllText(f),
                }).ToList(),
            };
            var prompt = PromptRenderer.Render(source, model);

            Directory.CreateDirectory(target);
            await File.WriteAllTextAsync(Path.Combine(target, name + ".prompt.md"), prompt, cancellationToken);

            LogAsk(log, options.Provider, directory, files.Count);
            var started = _time.GetUtcNow();
            var timestamp = _time.GetTimestamp();
            var response = await completion.GetCompletionAsync(options.Model!, prompt);
            var elapsed = _time.GetElapsedTime(timestamp);
            sent++;

            await File.WriteAllTextAsync(responseFile, response, cancellationToken);

            var extracted = options.ExtractFiles ? await WriteExtractedAsync(target, response, cancellationToken) : new List<string>();

            var metadata = new
            {
                provider = options.Provider,
                model = options.Model,
                template = name,
                folder = model.folder,
                inputFiles = files.Count,
                promptCharacters = prompt.Length,
                responseCharacters = response.Length,
                startedUtc = started,
                durationMilliseconds = (long)elapsed.TotalMilliseconds,
                extractedFiles = extracted,
            };
            await File.WriteAllTextAsync(Path.Combine(target, name + ".response.json"), JsonSerializer.Serialize(metadata, _json), cancellationToken);
        }

        return sent;
    }

    private async Task<List<string>> WriteExtractedAsync(string target, string response, CancellationToken cancellationToken)
    {
        var written = new List<string>();
        foreach (var file in ResponseFileExtractor.Extract(response))
        {
            var destination = Path.GetFullPath(Path.Combine(target, file.RelativePath));
            if (!destination.StartsWith(target + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                LogSkipOutside(log, file.RelativePath);
                continue;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            await File.WriteAllTextAsync(destination, file.Content, cancellationToken);
            written.Add(Path.GetRelativePath(target, destination).Replace('\\', '/'));
        }

        return written;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "skip {Directory}: response exists")]
    private static partial void LogSkipExisting(ILogger logger, string directory);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "ask {Provider} about {Directory} ({Count} files)")]
    private static partial void LogAsk(ILogger logger, string provider, string directory, int count);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "skip {Path}: outside the output folder")]
    private static partial void LogSkipOutside(ILogger logger, string path);
}
