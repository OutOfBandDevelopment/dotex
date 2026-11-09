using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Cgi;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OoBDev.WebApi.Services;

/// <summary>Lists the programs in the CGI script root that this host can start.</summary>
/// <param name="options">The CGI options (prefix, script root and handlers).</param>
public class CgiProgramCatalog(IOptions<CgiOptions> options)
{
    /// <summary>Lists the programs whose extension has a handler, skipping Linux binaries on Windows.</summary>
    /// <param name="isWindows">True when the host is Windows.</param>
    /// <returns>The programs ordered by name.</returns>
    public IReadOnlyList<CgiProgramModel> List(bool isWindows)
    {
        var cgi = options.Value;
        var root = Path.GetFullPath(cgi.ScriptRoot);
        if (!Directory.Exists(root))
            return [];

        var prefix = cgi.Prefix.TrimEnd('/');
        return
        [
            .. Directory.EnumerateFiles(root)
                .Where(file => cgi.Handlers.ContainsKey(Path.GetExtension(file)))
                .Where(file => !isWindows || !IsElf(file))
                .Select(file => Path.GetFileName(file))
                .Order(StringComparer.OrdinalIgnoreCase)
                .Select(name => new CgiProgramModel { Name = name, Path = $"{prefix}/{name}" }),
        ];
    }

    private static bool IsElf(string file)
    {
        Span<byte> head = stackalloc byte[4];
        using var stream = File.OpenRead(file);
        return stream.Read(head) == 4 && head[0] == 0x7F && head[1] == (byte)'E' && head[2] == (byte)'L' && head[3] == (byte)'F';
    }
}
