using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.PackMan.Cli;

internal static class Program
{
    private static string? ToolVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
    }

    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || Array.Exists(args, a => a is "-h" or "--help" or "-?"))
        {
            Console.WriteLine(PackManOptions.Usage);
            return 0;
        }

        try
        {
            var options = PackManOptions.Parse(args);
            if (options.GetVersion)
            {
                Console.WriteLine(ToolVersion());
                return 0;
            }

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var props = new PackageProps(loggerFactory.CreateLogger<PackageProps>());
            var versionXml = PackageProps.Open(options.SourceFile, options.FollowImports);

            if (!string.IsNullOrWhiteSpace(options.GetProperty))
            {
                Console.WriteLine(PackageProps.GetProperties(versionXml)
                    .Where(p => string.Equals(p.PropertyName, options.GetProperty, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Value)
                    .FirstOrDefault());
                return 0;
            }

            props.UpdateTargetVersions(versionXml, PackageProps.SplitVersions(options.TargetVersions));

            if (options.UpdateProjects)
            {
                foreach (var project in ProjectUpdater.Update(versionXml, options.ProjectsPath, options.Projects))
                    Console.WriteLine($"project: {project}");
            }

            if (!options.NoNuget)
            {
                var nugetLogger = new NuGetLoggerBridge(loggerFactory.CreateLogger<NuGetFeedVersionSource>());
                using var source = new NuGetFeedVersionSource(options.NugetUrl, options.FeedType, nugetLogger);
                await props.UpdateNugetVersionsAsync(versionXml, source, CancellationToken.None);
            }

            if (!options.NoSave)
                props.Save(versionXml, string.IsNullOrWhiteSpace(options.OutputFile) ? options.SourceFile : options.OutputFile);

            if (options.GetProperties)
            {
                foreach (var (name, value) in PackageProps.GetProperties(versionXml))
                    Console.WriteLine($"{name}={value}");
            }

            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or global::System.IO.IOException or global::System.Xml.XmlException or global::System.Net.Http.HttpRequestException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
