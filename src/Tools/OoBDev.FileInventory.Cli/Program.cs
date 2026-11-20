using System;
using System.Threading;

namespace OoBDev.FileInventory.Cli;

/// <summary>file-inventory: scan a folder and list duplicate files.</summary>
internal static class Program
{
    private static async System.Threading.Tasks.Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
        try
        {
            return await InventoryCommand.RunAsync(args, Console.Out, Console.Error, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return 2;
        }
    }
}
