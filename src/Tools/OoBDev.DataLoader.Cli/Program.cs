using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DataLoader.Cli;

/// <summary>data-loader: load a CSV or JSON file through the data loader pipeline. Exit code 0 clean, 1 row errors, 2 usage or file error.</summary>
internal static class Program
{
    private static async Task<int> Main(string[] args) => await LoaderCommand.RunAsync(args, Console.Out, Console.Error, CancellationToken.None).ConfigureAwait(false);
}
