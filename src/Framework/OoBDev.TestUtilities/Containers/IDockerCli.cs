using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.TestUtilities.Containers;

/// <summary>The few Docker operations the integration test container helper needs.</summary>
public interface IDockerCli
{
    /// <summary>Reads the state of a container, or null when it does not exist or Docker is not available.</summary>
    Task<ContainerState?> InspectAsync(string container, CancellationToken cancellationToken);

    /// <summary>Starts an existing, stopped container.</summary>
    Task StartAsync(string container, CancellationToken cancellationToken);

    /// <summary>Creates and starts one service of a compose file without its dependencies.</summary>
    Task ComposeUpAsync(string composeFile, string service, CancellationToken cancellationToken);

    /// <summary>Whether the container log contains the text; false when Docker is not available.</summary>
    Task<bool> LogContainsAsync(string container, string text, CancellationToken cancellationToken);

    /// <summary>Whether no container whose name starts with the prefix is running; null when Docker is not available.</summary>
    Task<bool?> NoneRunningAsync(string namePrefix, CancellationToken cancellationToken);

    /// <summary>Runs the PowerShell script that starts the stack in dependency waves; false when it failed or PowerShell is missing.</summary>
    Task<bool> StartWavesAsync(string scriptPath, CancellationToken cancellationToken);

    /// <summary>Stops a container.</summary>
    Task StopAsync(string container, TimeSpan grace, CancellationToken cancellationToken);
}
