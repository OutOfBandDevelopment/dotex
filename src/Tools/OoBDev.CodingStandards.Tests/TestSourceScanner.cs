using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.CodingStandards.Tests;

/// <summary>Finds test classes and their categories by reading source files, so no test assembly has to be referenced.</summary>
internal static partial class TestSourceScanner
{
    [GeneratedRegex("""\bTestCategory\(\s*(?:"(?<name>[^"]+)"|(?<const>[A-Za-z_.]+))\s*\)""")]
    private static partial Regex CategoryPattern();

    [GeneratedRegex(@"(?<attrs>(?:[ \t]*\[[^\r\n]*\][ \t]*\r?\n)*)[ \t]*(?:public|internal)\s+(?:(?:sealed|abstract|static|partial)\s+)*class\s+(?<name>\w+)")]
    private static partial Regex ClassPattern();

    [GeneratedRegex(@"(?<attrs>(?:[ \t]*\[[^\r\n]*\][ \t]*\r?\n)+)[ \t]*(?:public|internal)\s+[\w<>\[\]?,. ]+?\s+(?<name>\w+)\s*\(")]
    private static partial Regex MethodPattern();

    /// <summary>Every <c>[TestClass]</c> under <paramref name="folder"/>, skipping build output.</summary>
    public static IEnumerable<TestClassInfo> Scan(string root, string folder)
    {
        foreach (var path in Directory.EnumerateFiles(Path.Combine(root, folder), "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
            if (relative.Contains("/obj/", StringComparison.Ordinal) || relative.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            var text = File.ReadAllText(path);
            if (!text.Contains("[TestClass", StringComparison.Ordinal))
            {
                continue;
            }

            var classes = ClassPattern().Matches(text);
            for (var i = 0; i < classes.Count; i++)
            {
                var match = classes[i];
                if (!match.Groups["attrs"].Value.Contains("[TestClass", StringComparison.Ordinal))
                {
                    continue;
                }

                var next = classes.Skip(i + 1).FirstOrDefault(c => c.Groups["attrs"].Value.Contains("[TestClass", StringComparison.Ordinal));
                var end = next?.Index ?? text.Length;
                var body = text[match.Index..end];
                var methods = MethodPattern().Matches(body)
                    .Where(m => m.Groups["attrs"].Value.Contains("TestMethod", StringComparison.Ordinal))
                    .Select(m => new TestMethodInfo(m.Groups["name"].Value, Categories(m.Groups["attrs"].Value)))
                    .ToList();
                if (methods.Count == 0)
                {
                    continue;
                }

                yield return new TestClassInfo(relative, match.Groups["name"].Value, Categories(match.Groups["attrs"].Value), methods);
            }
        }
    }

    private static List<string> Categories(string attributes) =>
        [.. CategoryPattern().Matches(attributes).Select(m => m.Groups["name"].Success ? m.Groups["name"].Value : m.Groups["const"].Value.Split('.')[^1])];
}
