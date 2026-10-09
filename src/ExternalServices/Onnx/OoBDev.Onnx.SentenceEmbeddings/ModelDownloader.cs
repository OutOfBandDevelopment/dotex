using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// Downloads missing model files into the model folder on first use. Safe for several processes that share the folder
/// (for example a mapped volume): a lock file serialises the download, and a file only appears once its hash is verified.
/// </summary>
internal static partial class ModelDownloader
{
    private static readonly HttpClient _client = new() { Timeout = Timeout.InfiniteTimeSpan };

    public static async Task EnsureAsync(string folder, IEnumerable<ModelFileSource> sources, ILogger logger, CancellationToken cancellationToken)
    {
        foreach (var source in sources)
        {
            var target = Path.Combine(folder, source.FileName);
            if (File.Exists(target)) continue;

            Directory.CreateDirectory(folder);
            await using var gate = await AcquireAsync(Path.Combine(folder, source.FileName + ".lock"), cancellationToken).ConfigureAwait(false);
            if (File.Exists(target)) continue; // another process finished while this one waited

            var temp = Path.Combine(folder, $"{source.FileName}.{Guid.NewGuid():N}.download");
            try
            {
                LogDownloading(logger, source.Url, target);
                using (var response = await _client.GetAsync(source.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    await using var network = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    await using var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                    await network.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
                }

                string hash;
                await using (var verify = File.OpenRead(temp))
                    hash = Convert.ToHexString(await SHA256.HashDataAsync(verify, cancellationToken).ConfigureAwait(false));
                if (!string.Equals(hash, source.Sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"The download of {source.FileName} from {source.Url} has SHA-256 {hash}, expected {source.Sha256}.");

                File.Move(temp, target);
                LogDownloaded(logger, target);
            }
            finally
            {
                if (File.Exists(temp)) File.Delete(temp);
            }
        }
    }

    private static async Task<IAsyncDisposable> AcquireAsync(string lockFile, CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                return new Lock(new FileStream(lockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose));
            }
            catch (IOException)
            {
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private sealed class Lock(FileStream stream) : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => stream.DisposeAsync();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Downloading embedding model file {Url} to {Target}")]
    private static partial void LogDownloading(ILogger logger, string url, string target);

    [LoggerMessage(Level = LogLevel.Information, Message = "Downloaded and verified {Target}")]
    private static partial void LogDownloaded(ILogger logger, string target);
}
