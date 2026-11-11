using Microsoft.Extensions.Configuration;

namespace OoBDev.MigrationHelper.Cli;

internal sealed class Program
{
    private const string Usage = """
        oobdev-migrationhelper --path <folder> [--from BinaryDataDecoders] [--to OoBDev] [--dry-run]
          Renames folders and files that start with the --from prefix and replaces the prefix inside every text file under --path.
          It changes files in place, so run it on a clean working tree only. Files with a null character are skipped.
          --path     folder to rewrite (required)
          --from     prefix to replace (default BinaryDataDecoders)
          --to       new prefix (default OoBDev)
          --dry-run  list what would change and change nothing
        """;

    static int Main(string[] args)
    {
        var options = new ConfigurationBuilder().AddCommandLine(args).Build();
        var help = args.Any(a => a is "-h" or "--help" or "-?");
        if (help || string.IsNullOrWhiteSpace(options["path"]))
        {
            Console.WriteLine(Usage);
            return help ? 0 : 1;
        }

        var dryRun = args.Any(a => string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase));
        PrefixMigrator.Run(options["path"]!, options["from"] ?? "BinaryDataDecoders", options["to"] ?? "OoBDev", dryRun, Console.Out, Console.Error);
        return 0;
    }
}
