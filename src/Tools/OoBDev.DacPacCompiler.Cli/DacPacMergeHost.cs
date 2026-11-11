using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OoBDev.Microsoft.SqlServer.DacFx;
using OoBDev.System;
using OoBDev.System.Text;

namespace OoBDev.DacPacCompiler.Cli;

/// <summary>
/// Selects the tool from the command line and builds the host for the merge tool.
/// </summary>
internal static class DacPacMergeHost
{
    /// <summary>
    /// Reads the <c>--tool</c> value (<c>--tool Merge</c> or <c>--tool=Merge</c>); <see cref="DacPackTools.SqlClr"/> when absent.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static DacPackTools SelectTool(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            string value;
            if (arg.Equals("--tool", StringComparison.OrdinalIgnoreCase))
                value = i + 1 < args.Length ? args[i + 1] : string.Empty;
            else if (arg.StartsWith("--tool=", StringComparison.OrdinalIgnoreCase))
                value = arg["--tool=".Length..];
            else
                continue;

            return Enum.TryParse<DacPackTools>(value, ignoreCase: true, out var tool) ? tool : DacPackTools.Unknown;
        }

        return DacPackTools.SqlClr;
    }

    /// <summary>
    /// Builds the host that merges DACPACs. Options use the switches of <see cref="DacPacCompilerConfig.CommandLineSwitchMappings"/>.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static IHostBuilder Create(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) => config.AddCommandLine(args, DacPacCompilerConfig.CommandLineSwitchMappings))
            .ConfigureServices((_, services) =>
            {
                services.AddLogging(options => options.AddFilter("Microsoft", LogLevel.Warning));
                services.TrySerializerExtensions(SerializerTypes.Json);
                services.AddDacPacCompilerServices();
                services.AddHostedService<DacPacMergeEngineService>();
            });

    /// <summary>The usage text of the merge tool.</summary>
    public const string Usage = """
        dacpactools --tool Merge --target-path <file> [options]

        Merges several DACPACs into one package.

          -t, --template <file>        Template file read through the object converter (JSON); options below override it.
          -s, --source-path <folder>   Folder the source patterns are resolved against (default: current folder).
          -p, --source-patterns <list> Semicolon separated glob patterns (default: *.dacpac); a leading ! excludes.
          -r, --target-path <file>     The merged DACPAC to write.
          -n, --name <text>            Package name (default: the target file name).
          -d, --description <text>     Package description (default: the local time).
          -v, --version <text>         Package version (default: 0.0.0).
          -b, --build-version <text>   Build version written by the post-deployment script (default: the version).
              --setting-modeloptionsource <Custom|First|Last>
                                       Where the model options come from.
        """;
}
