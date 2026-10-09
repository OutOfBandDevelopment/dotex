using Microsoft.Extensions.Logging;
using OoBDev.AI;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.LlmCodeGen.Cli;

/// <summary>
/// Walks the input folders, renders one prompt per folder, asks the model and saves the response and the files in it.
/// </summary>
/// <param name="completion">The selected model provider.</param>
/// <param name="log">Diagnostic output.</param>
public class FolderPromptRunner(IMessageCompletion completion, ILogger<FolderPromptRunner> log)
{
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
                log.LogInformation("skip {Directory}: response exists", directory);
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

            log.LogInformation("ask {Provider} about {Directory} ({Count} files)", options.Provider, directory, files.Count);
            var response = await completion.GetCompletionAsync(options.Model!, prompt);
            sent++;

            await File.WriteAllTextAsync(responseFile, response, cancellationToken);

            if (options.ExtractFiles)
            {
                await WriteExtractedAsync(target, response, cancellationToken);
            }
        }

        return sent;
    }

    private async Task WriteExtractedAsync(string target, string response, CancellationToken cancellationToken)
    {
        foreach (var file in ResponseFileExtractor.Extract(response))
        {
            var destination = Path.GetFullPath(Path.Combine(target, file.RelativePath));
            if (!destination.StartsWith(target + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                log.LogWarning("skip {Path}: outside the output folder", file.RelativePath);
                continue;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            await File.WriteAllTextAsync(destination, file.Content, cancellationToken);
        }
    }
}
