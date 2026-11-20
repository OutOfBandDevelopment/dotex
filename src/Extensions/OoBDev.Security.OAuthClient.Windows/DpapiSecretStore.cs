using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Security.OAuthClient;

/// <summary>Keeps secrets encrypted with Windows DPAPI for the current user, so a copied file is useless on another account or machine.</summary>
[SupportedOSPlatform("windows")]
public sealed class DpapiSecretStore : ISecretStore
{
    private readonly string _folder;

    /// <summary>Creates the store.</summary>
    /// <param name="folder">The folder to keep files in; defaults to <c>%LOCALAPPDATA%\OoBDev\oauth</c>.</param>
    public DpapiSecretStore(string? folder = null) =>
        _folder = folder ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OoBDev", "oauth");

    /// <inheritdoc />
    public async Task<string?> ReadAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = PathFor(name);
        if (!File.Exists(path))
            return null;
        var cipher = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        try
        {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(cipher, null, DataProtectionScope.CurrentUser));
        }
        catch (CryptographicException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task WriteAsync(string name, string value, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_folder);
        var path = PathFor(name);
        var temp = path + ".tmp";
        await File.WriteAllBytesAsync(temp, ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser), cancellationToken).ConfigureAwait(false);
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
        return Path.Combine(_folder, name + ".dpapi");
    }
}
