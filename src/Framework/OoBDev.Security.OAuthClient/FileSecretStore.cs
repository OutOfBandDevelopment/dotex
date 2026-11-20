using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OoBDev.Security.OAuthClient;

/// <summary>Keeps secrets as files in the user's profile. The folder is private to the user where the platform allows it; use a protected store (for example DPAPI on Windows) when the machine is shared.</summary>
public sealed class FileSecretStore : ISecretStore
{
    private readonly string _folder;

    /// <summary>Creates the store.</summary>
    /// <param name="options">The client options; <see cref="OAuthClientOptions.SecretFolder"/> picks the folder.</param>
    public FileSecretStore(IOptions<OAuthClientOptions> options)
        : this(options?.Value.SecretFolder ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OoBDev", "oauth"))
    {
    }

    /// <summary>Creates the store on a folder.</summary>
    /// <param name="folder">The folder to keep files in.</param>
    public FileSecretStore(string folder) => _folder = folder;

    /// <inheritdoc />
    public async Task<string?> ReadAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = PathFor(name);
        return File.Exists(path) ? await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false) : null;
    }

    /// <inheritdoc />
    public async Task WriteAsync(string name, string value, CancellationToken cancellationToken = default)
    {
        var path = PathFor(name);
        var directory = Directory.CreateDirectory(_folder);
        if (!OperatingSystem.IsWindows())
            directory.UnixFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
        var temp = path + ".tmp";
        await File.WriteAllTextAsync(temp, value, cancellationToken).ConfigureAwait(false);
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(temp, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        File.Move(temp, path, overwrite: true);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = PathFor(name);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    private string PathFor(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.AsSpan().IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains("..", StringComparison.Ordinal))
            throw new ArgumentException("The secret name must be a plain file name.", nameof(name));
        return Path.Combine(_folder, name + ".secret");
    }
}
