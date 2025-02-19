using OoBDev.System.Text.Templating;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OoBDev.TemplateEngine.Cli;

public class TemplateEngineService(
    ILogger<TemplateEngineService> log,
    IOptions<TemplateEngineOptions> settings,
    ITemplateEngine engine
        ) : IHostedService
{
    private readonly ILogger _log = log;
    private readonly IOptions<TemplateEngineOptions> _settings = settings;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
        if (string.IsNullOrWhiteSpace(_settings.Value.InputFile)) throw new ArgumentNullException(nameof(_settings.Value.InputFile));
        if (string.IsNullOrWhiteSpace(_settings.Value.Template)) throw new ArgumentNullException(nameof(_settings.Value.Template));
        if (string.IsNullOrWhiteSpace(_settings.Value.OutputFile)) throw new ArgumentNullException(nameof(_settings.Value.OutputFile));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

        var inputPath = Path.GetFullPath(Path.GetDirectoryName(_settings.Value.InputFile) ?? ".");
        var inputFile = Path.GetFileName(_settings.Value.InputFile);

        foreach (var file in Directory.EnumerateFiles(inputPath, inputFile))
        {
            var contentType = GetFileType(_settings.Value.InputFileType, file) ??
                throw new NotSupportedException(@$"""{file}"" => ""{_settings.Value.InputFileType}""");
            var content = GetContent(file) ??
                throw new NotSupportedException($"No content found: \"{file}\"");
            var data = GetData(contentType, content) ??
                throw new NotSupportedException($"No data found: ({contentType}:{content?.Length})"); ;

            _log.LogInformation(
                $"Loaded: {{{nameof(file)}}} for {{{nameof(_settings.Value.Template)}}}",
                _settings.Value.InputFile,
                _settings.Value.Template
                );

            var outFile = _settings.Value.OutputFile
                .Replace("[file]", Path.GetFileNameWithoutExtension(file), StringComparison.InvariantCultureIgnoreCase)
                ;
            var dir = Path.GetDirectoryName(outFile);
            if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            using var fileWriter = File.Open(outFile, FileMode.Create, FileAccess.Write, FileShare.Read);

            var result = await engine.ApplyAsync(_settings.Value.Template, data, fileWriter);
            await fileWriter.FlushAsync(cancellationToken);
            _log.LogInformation($"Written: {{{nameof(outFile)}}}", outFile);
        }
    }

    private static string? GetContent(string fileName) => File.Exists(fileName) ? File.ReadAllText(fileName) : null;
    private static FileTypes? GetFileType(FileTypes? fileTypes, string fileName) =>
       fileTypes ?? Path.GetExtension(fileName)?.ToLowerInvariant() switch
       {
           ".json" => FileTypes.Json,
           ".xml" => FileTypes.Xml,
           _ => null
       };

    private static object? GetData(FileTypes type, string content) =>
        type switch
        {
            FileTypes.Json => JsonDocument.Parse(content),
            FileTypes.Xml => XDocument.Parse(content),
            _ => null,
        };

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
