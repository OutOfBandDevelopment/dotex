using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace OoBDev.FixSourceLinks.Cli;

internal class Program
{
    private static void Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build()
            )
            ;

        var serviceProvider = services.BuildServiceProvider();

        var config = serviceProvider.GetRequiredService<IConfiguration>();

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);

        matcher.AddIncludePatterns(config["include"]?.Split(';') ?? []);
        matcher.AddExcludePatterns(config["exclude"]?.Split(';') ?? []);

        var search = Path.GetFullPath(config["search"] ?? Environment.CurrentDirectory);
        var target = Path.GetFullPath(config["target"] ?? search);

        var files = matcher.GetResultsInFullPath(search);

        Console.WriteLine($"include: \"{config["include"]}\"");
        Console.WriteLine($"exclude: \"{config["exclude"]}\"");
        Console.WriteLine($"search: \"{config["search"]}\" => \"{search}\"");
        Console.WriteLine($"target: \"{config["target"]}\" => \"{target}\"");

        if (!files.Any())
        {
            Console.WriteLine($"No Matches!");
            foreach (var item in Directory.EnumerateDirectories(search))
            {
                Console.WriteLine($"directory: {item}");
            }
            foreach (var item in Directory.EnumerateFiles(search))
            {
                Console.WriteLine($"file: {item}");
            }
        }
        else
        {
            foreach (var file in files)
            {
                Console.WriteLine(file);
                try
                {
                    var xml = XDocument.Load(file);

                    var attributes = (
                        from element in xml.Descendants()
                        let attribute = element.Attribute("filename")
                        where attribute != null
                        let existingPath = attribute?.Value
                        where !string.IsNullOrWhiteSpace(existingPath)
                        where existingPath.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                        let uri = existingPath == null ? null : new Uri(existingPath)
                        where !string.IsNullOrWhiteSpace(uri.Query)
                        let path = uri == null ? null : HttpUtility.ParseQueryString(uri.Query)?["path"]?.TrimStart('/', '\\')
                        where !string.IsNullOrWhiteSpace(path)
                        let resultPath = path == null ? null : Path.GetFullPath(Path.Combine(target, path))
                        select new
                        {
                            attribute,
                            existingPath,
                            uri,
                            path,
                            resultPath,
                        }
                    ).ToList();

                    var changes = false;
                    foreach (var item in attributes)
                    {
                        if (item.attribute != null && item.resultPath != null)
                        {
                            changes = true;
                            Console.WriteLine($"\tValue: \"{item.attribute.Value}\" to  \"{item.resultPath}\"");
                            item.attribute.Value = item.resultPath;
                        }
                        else
                        {
                            Console.WriteLine($"\tMissing: \"{item.attribute?.Value}\" to  \"{item.resultPath}\"");
                        }
                    }

                    if (changes)
                        xml.Save(file);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(ex.ToString());
                }
            }
        }
    }
}
