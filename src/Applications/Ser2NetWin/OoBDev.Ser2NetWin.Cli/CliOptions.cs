using OoBDev.System.IO.Ports.Bridge;
namespace OoBDev.Ser2NetWin.Cli;

public sealed class CliOptions
{
    public string Command { get; private init; } = "run";

    public string? ConfigPath { get; private init; }

    public bool StartAfterInstall { get; private init; }

    public string? Error { get; private init; }

    public string ResolvedConfigPath => ConfigPath ?? ConfigStore.DefaultPath;

    public static CliOptions Parse(IReadOnlyList<string> args)
    {
        string? command = null;
        string? configPath = null;
        var start = false;

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "-c" or "--config":
                    if (i + 1 >= args.Count)
                    {
                        return new CliOptions { Error = $"{arg} requires a path." };
                    }

                    configPath = args[++i];
                    break;
                case "--start":
                    start = true;
                    break;
                case "-h" or "--help" or "-?":
                    command = "help";
                    break;
                default:
                    if (arg.StartsWith('-'))
                    {
                        return new CliOptions { Error = $"Unknown option '{arg}'." };
                    }

                    if (command is not null)
                    {
                        return new CliOptions { Error = $"Unexpected argument '{arg}'." };
                    }

                    command = arg.ToLowerInvariant();
                    break;
            }
        }

        return new CliOptions
        {
            Command = command ?? "run",
            ConfigPath = configPath,
            StartAfterInstall = start,
        };
    }
}
