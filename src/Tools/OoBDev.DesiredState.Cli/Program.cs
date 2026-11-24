using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Data.DesiredState;

namespace OoBDev.DesiredState.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection().AddDesiredStateDatabase();
        using var provider = services.BuildServiceProvider();
        return await DesiredStateCommand.RunAsync(args, provider.GetRequiredService<DesiredStateProviderFactory>(), Environment.GetEnvironmentVariable, Console.Out, Console.Error).ConfigureAwait(false);
    }
}
