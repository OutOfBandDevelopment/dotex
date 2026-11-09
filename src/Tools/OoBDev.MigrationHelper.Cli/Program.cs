using Microsoft.Extensions.Configuration;

namespace OoBDev.MigrationHelper.Cli;

internal sealed class Program
{
    private const string Usage = """
        oobdev-migrationhelper --path <folder> [--from BinaryDataDecoders] [--to OoBDev]
          Renames folders and files that start with the --from prefix and replaces the prefix inside every text file under --path.
          It changes files in place, so run it on a clean working tree only.
          --path  folder to rewrite (required)
          --from  prefix to replace (default BinaryDataDecoders)
          --to    new prefix (default OoBDev)
        """;

    static int Main(string[] args)
    {
        var options = new ConfigurationBuilder().AddCommandLine(args).Build();
        if (args.Any(a => a is "-h" or "--help" or "-?") || string.IsNullOrWhiteSpace(options["path"]))
        {
            Console.WriteLine(Usage);
            return args.Any(a => a is "-h" or "--help" or "-?") ? 0 : 1;
        }

        Run(options["path"]!, options["from"] ?? "BinaryDataDecoders", options["to"] ?? "OoBDev");
        return 0;
    }

    private static void Run(string path, string sourcePrefix, string targetPrefix)
    {

        foreach (var folder in Directory.EnumerateDirectories(path, "*.*", SearchOption.AllDirectories))
        {
            var dir = Path.GetDirectoryName(folder) ?? ".";
            var current = Path.GetFileName(folder);
            if (current.StartsWith(sourcePrefix, StringComparison.Ordinal))
            {
                var next = Path.Combine(dir, current.Replace(sourcePrefix, targetPrefix));
                Console.WriteLine($"{current}");
                Directory.Move(folder, next);
            }
        }

        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
        {
            var dir = Path.GetDirectoryName(file) ?? ".";
            var current = Path.GetFileName(file);
            if (current.StartsWith(sourcePrefix, StringComparison.Ordinal))
            {
                var next = Path.Combine(dir, current.Replace(sourcePrefix, targetPrefix));
                Console.WriteLine($"{current}");
                File.Move(file, next);
            }
        }

        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
        {
            try
            {
                var content = File.ReadAllText(file);

                if (!content.Contains(sourcePrefix)) continue;

                content = content.Replace(sourcePrefix, targetPrefix);

                if (content.Contains('\0'))
                {
                    Console.WriteLine($"Skip: {file}");
                }

                Console.WriteLine($"{file}");
                File.WriteAllText(file, content);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ex.Message}");
            }
        }
    }
}
