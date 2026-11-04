using System;
using System.IO;

namespace OoBDev.CodingStandards.Tests;

internal static class RepositoryRoot
{
    /// <summary>Walks up from the test output to the folder holding <c>CLAUDE.md</c>.</summary>
    public static string Find()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CLAUDE.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root (CLAUDE.md) not found.");
    }
}
