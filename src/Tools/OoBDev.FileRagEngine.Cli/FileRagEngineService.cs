using OoBDev.AI;
using OoBDev.System.Text.Templating;
using HandlebarsDotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.FileRagEngine.Cli;

public class FileRagEngineService : IHostedService
{
    private readonly ILogger _log;
    private readonly IOptions<FileRagEngineOptions> _settings;
    private readonly ITemplateEngine _engine;
    private readonly IServiceProvider _serviceProvider;

    public FileRagEngineService(
        ILogger<FileRagEngineService> log,
        IOptions<FileRagEngineOptions> settings,
        ITemplateEngine engine,
        IServiceProvider serviceProvider
        )
    {
        _log = log;
        _settings = settings;
        _engine = engine;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
        if (string.IsNullOrWhiteSpace(_settings.Value.InputPath)) throw new ArgumentNullException(nameof(_settings.Value.InputPath));
        if (string.IsNullOrWhiteSpace(_settings.Value.Template)) throw new ArgumentNullException(nameof(_settings.Value.Template));
        if (string.IsNullOrWhiteSpace(_settings.Value.OutputPath)) throw new ArgumentNullException(nameof(_settings.Value.OutputPath));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

        //app starts here
        var inputPath = Path.GetFullPath(_settings.Value.InputPath);
        var outputPath = Path.GetFullPath(_settings.Value.OutputPath);

        // create a connection to ollama
        var client = GetProvider();

        // scan directories from application
        var directories = Directory.GetDirectories(inputPath, "*.*", SearchOption.AllDirectories);

        var exceptions = new List<Exception>();

        foreach (var directory in directories)
        {
            var realative = Path.GetFullPath(directory).Replace(inputPath + "\\", "");

            if (new[] { @"\bin\", @"\obj\" }.Any(i => directory.Contains(i, StringComparison.OrdinalIgnoreCase)) ||
                new[] { @"\bin", @"\obj" }.Any(i => directory.EndsWith(i, StringComparison.OrdinalIgnoreCase)))
            {
                _log.LogWarning("skip path: {directory}", directory);
                continue;
            }

            _log.LogInformation("reading: {directory}", directory);

            var outFolder = Path.Combine(outputPath, realative);

            var files = (from file in Directory.GetFiles(directory)
                         let ext = Path.GetExtension(file).ToUpper() //TODO: do something smarter
                         where !new[] {
                             ".PDF", ".DLL", ".EXE", ".PNG", ".GIF", ".JPG", ".ZIP", ".GZ", ".EPUB", ".RTF", ".DOC", ".DOCX" ,
                             ".V2", ".DACPAC", ".BACPAC", ".SCMP", ".BIN", ".SVG", ".ICO",
                         }.Any(i => i == ext)
                         select file).ToArray();

            if (!files.Any())
            {
                _log.LogWarning("path empty: {directory}", directory);
                continue;
            }
            if (!Path.Exists(outFolder)) Directory.CreateDirectory(outFolder);

            var responseFileContent = Path.Combine(outFolder, _settings.Value.Template);
            var promptFile = Path.ChangeExtension(responseFileContent, ".prompt" + Path.GetExtension(responseFileContent));
            var responseFile = Path.ChangeExtension(responseFileContent, ".json");

            if (!File.Exists(responseFileContent))
            {
                var length = 5;
                for (var x = 0; x < files.Length; x += length)
                {
                    var realResponseFileContent = x == 0 ? responseFileContent : Path.ChangeExtension(responseFileContent, $".{x}" + Path.GetExtension(responseFileContent));

                    try
                    {
                        var data = new
                        {
                            files = files.Skip(x).Take(length).Select(file => new { name = Path.GetFileName(file), content = File.ReadAllText(file) }).ToArray(),
                        };

                        // per folder generate a prompt using handlebars
                        _log.LogInformation("generate prompt: {directory}", directory);

                        using var promptStream = new MemoryStream();
                        var templateContext = await _engine.ApplyAsync(_settings.Value.Template, data, promptStream);
                        promptStream.Position = 0;
                        var promptReader = new StreamReader(promptStream);

                        if (_settings.Value.IncludePrompt)
                        {
                            var realPromptFile = x == 0 ? promptFile : Path.ChangeExtension(promptFile, $".{x}" + Path.GetExtension(promptFile));
                            await File.WriteAllBytesAsync(realPromptFile, promptStream.ToArray());
                        }

                        // post prompt to ollama
                        _log.LogInformation("request completion: {directory}", directory);
                        var response = await client.GetCompletionAsync(new() { Prompt = promptReader.ReadToEnd() });

                        // capture response from ollama
                        _log.LogInformation("write files: {directory}", directory);
                        await File.WriteAllTextAsync(realResponseFileContent, response.Response);

                        if (_settings.Value.IncludeRawOutput)
                        {
                            using var responseStream = File.Create(responseFile);
                            await JsonSerializer.SerializeAsync(responseStream, response);
                            await responseStream.FlushAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError("ERROR: {directory} ({x}) -> {error}", directory, x, ex.Message);
                        _log.LogDebug("Exception: {exception}", ex);

                        var errorFile = Path.ChangeExtension(realResponseFileContent, ".error" + Path.GetExtension(responseFileContent));
                        await File.WriteAllTextAsync(errorFile, ex.ToString());
                        exceptions.Add(ex);
                    }
                }
            }
        }
        if (exceptions.Any())
            throw new AggregateException(exceptions);
    }

    private IMessageCompletion GetProvider() => _settings.Value.LanguageModelType switch
    {
        string key => _serviceProvider.GetKeyedService<IMessageCompletion>(_settings.Value.LanguageModelType),
        _ => _serviceProvider.GetService<IMessageCompletion>()
    } ?? throw new NotSupportedException($"No language model provider configured");

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
