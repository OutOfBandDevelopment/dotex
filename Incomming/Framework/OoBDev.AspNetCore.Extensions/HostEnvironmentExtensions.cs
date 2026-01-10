using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace OoBDev.AspNetCore.Extensions;

/// <summary>
/// Extensions for IHostEnvironment
/// </summary>
public static class HostEnvironmentExtensions
{
    /// <summary>
    /// method to check if application is running as Local
    /// </summary>
    /// <param name="hostEnvironment"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool IsLocal(this IHostEnvironment hostEnvironment) =>
        hostEnvironment?.IsEnvironment("Local") ?? false;
}
