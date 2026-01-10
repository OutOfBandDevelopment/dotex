using GroqNet;
using GroqNet.ChatCompletions;
using HandlebarsDotNet;
using System.Text;
using System.Text.Json;

namespace OobDev.BulkLlm.GroqNet.Cli;

internal class Program
{
    static async Task Main(string[] args)
    {
        //var inputPath = @"C:\Repos\Nucleus\Net.Api";
        var inputPath = @"C:\Repos\Nucleus\Net.Libs\docs";
        var outputPath = @".\Net.Core\src";
        var promptTemplateSource = @".\Prompts\GenerateDocumentationForThisCode.md.hbs"; // @".\Prompts\Angular2React.md.hbs";

        var apiKey = Environment.GetEnvironmentVariable("API_Key_Groq", EnvironmentVariableTarget.User);

        //app starts here
        inputPath = Path.GetFullPath(inputPath);
        outputPath = Path.GetFullPath(outputPath);
        var promptTemplate = File.ReadAllText(promptTemplateSource);

        // create a connection to groq
        var client = new GroqClient(apiKey, GroqModel.LLaMA3_8b);

        // scan directories from application
        var directories = Directory.GetDirectories(inputPath, "*.*", SearchOption.AllDirectories);
        foreach (var directory in directories)
        {
            var realative = Path.GetFullPath(directory).Replace(inputPath + "\\", "");

            Console.WriteLine($"reading: {directory}");

            var outFolder = Path.Combine(outputPath, realative);

            var files = Directory.GetFiles(directory).Where(f => new FileInfo(f).Length < 10000 && Path.GetExtension(f).ToUpperInvariant() != ".PDF").ToArray();

            if (!files.Any()) continue;
            if (!Path.Exists(outFolder)) Directory.CreateDirectory(outFolder);

            var promptFile = Path.Combine(outFolder, Path.GetFileNameWithoutExtension(promptTemplateSource));
            var responseFileContent = Path.ChangeExtension(promptFile, ".response" + Path.GetExtension(promptFile));
            var responseFile = Path.ChangeExtension(promptFile, ".response.json");

            if (!File.Exists(responseFileContent))
            {
                // per folder generate a prompt using handlebars
                Console.WriteLine($"generate prompt: {directory}");
                var prompt = await CreatePromptAsync(
                    promptTemplate,
                    new
                    {
                        date = DateTime.Now,
                        files = (from file in files
                                 let content = File.ReadAllText(file)
                                 where content.Length > 5
                                 where content[1..3] != "PNG"
                                 select new
                                 {
                                     name = Path.GetFileName(file),
                                     content = content
                                 }).ToArray(),
                    },
                    new { }
                    );
                await File.WriteAllTextAsync(promptFile, prompt);

                // post prompt to groq
                Console.WriteLine($"request completion: {directory}");
                var response = await client.GetChatCompletionsAsync(new GroqChatHistory { new(prompt) });

                // capture response from groq
                Console.WriteLine($"write files: {directory}");
                await File.WriteAllTextAsync(responseFileContent, response.Choices.First().Message.Content);

                using var responseStream = File.Create(responseFile);
                await JsonSerializer.SerializeAsync(responseStream, response);
                await responseStream.FlushAsync();
            }

            //// process responses to generate new application
            //Console.WriteLine($"create output: {directory}");
            //if (File.Exists(responseFileContent))
            //{
            //    var responseContent = await File.ReadAllTextAsync(responseFileContent);
            //    await ProcessResponseContentAsync(responseContent, outFolder);
            //}
        }
    }

    public static async Task ProcessResponseContentAsync(string responseContent, string basePath)
    {
        var reader = await GetTextReaderAsync(writer => writer.WriteAsync(responseContent));

        string? outFile = null;
        var stringBuilder = new StringBuilder();
        var readingContent = false;
        string? peeked = null;
        var unknown = 0;

        for (string? line = ""; line != null; line = peeked ?? await reader.ReadLineAsync(), peeked = null)
        {
            if (!readingContent)
            {
                if (line.StartsWith("**"))
                {
                    outFile = line.Trim('*');

                    if (outFile.StartsWith('`'))
                    {
                        outFile = outFile.Split('`')[1];
                    }
                }

                if (line.StartsWith("```"))
                {
                    readingContent = true;
                    peeked = await reader.ReadLineAsync();
                    if (peeked?.StartsWith("//") ?? false)
                    {
                        outFile = peeked[2..].Trim();
                        peeked = null;
                    }
                }
            }
            else if (line.StartsWith("```"))
            {
                readingContent = false;

                if (outFile?.Intersect(Path.GetInvalidFileNameChars()).Any() ?? false)
                {
                    stringBuilder.Insert(0, "// " + outFile + Environment.NewLine);
                    outFile = $"error{unknown++}.txt";
                }

                outFile ??= $"unknown{unknown++}.txt";
                var realFile = Path.Combine(basePath, outFile);
                var realDirectory = Path.GetDirectoryName(realFile);
                if (!Path.Exists(realDirectory)) Directory.CreateDirectory(realDirectory);

                await File.WriteAllTextAsync(realFile, stringBuilder.ToString());
                outFile = null;
            }
            else
            {
                stringBuilder.AppendLine(line);
            }
        }
    }

    public static async Task<string> CreatePromptAsync(string template, object data, object context)
    {
        var handlebars = Handlebars.Create(new HandlebarsConfiguration()
        {
            NoEscape = true,
        });

        using var templateReader = await GetTextReaderAsync(async writer => await writer.WriteLineAsync(template));
        var compiled = handlebars.Compile(templateReader);

        using var outputReader = await GetTextReaderAsync(writer => compiled(writer, context: data, data: context));
        var output = outputReader.ReadToEnd();
        return output;
    }

    public static async Task<TextReader> GetTextReaderAsync(Action<StreamWriter> action) =>
        await GetTextReaderAsync(async writer =>
        {
            action(writer);
            await Task.CompletedTask;
        });

    public static async Task<TextReader> GetTextReaderAsync(Func<StreamWriter, Task> action)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true)
        {
            AutoFlush = true,
        };
        await action(writer);
        await writer.FlushAsync();
        stream.Position = 0;
        var reader = new StreamReader(stream);

        return reader;
    }
}
