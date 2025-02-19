using Microsoft.Extensions.DependencyInjection;
using System;

namespace OoBDev.System.IO;

/// <summary>
/// Represents a factory for creating temporary files.
/// </summary>
public class TempFileFactory : ITempFileFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TempFileFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used for creating instances.</param>
    public TempFileFactory(
        IServiceProvider serviceProvider
        ) => _serviceProvider = serviceProvider;

    /// <summary>
    /// Creates a new temporary file.
    /// </summary>
    /// <returns>A temporary file.</returns>
    public ITempFile GetTempFile() =>
        ActivatorUtilities.CreateInstance<TempFileHandle>(_serviceProvider);
}
