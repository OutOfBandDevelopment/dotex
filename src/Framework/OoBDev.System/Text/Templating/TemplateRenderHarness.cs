using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Renders templates against sample data and compares the output with the approved text, so a template or engine
/// change that alters a message shows up as a failing test. Line endings and trailing blank lines are ignored.
/// </summary>
public static class TemplateRenderHarness
{
    /// <summary>Renders every case and returns the ones that do not match.</summary>
    /// <param name="engine">The engine under test.</param>
    /// <param name="cases">The templates, sample data and approved output.</param>
    /// <returns>The failures; empty when every case matches.</returns>
    public static async Task<IReadOnlyList<TemplateRenderFailure>> CheckAsync(ITemplateEngine engine, IEnumerable<TemplateRenderCase> cases)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(cases);

        var failures = new List<TemplateRenderFailure>();
        foreach (var testCase in cases)
        {
            if (engine.Get(testCase.TemplateName) == null)
            {
                failures.Add(new(testCase.TemplateName, "No template with this name is registered."));
                continue;
            }

            string? actual;
            try
            {
                actual = await engine.ApplyAsync(testCase.TemplateName, testCase.Data).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                failures.Add(new(testCase.TemplateName, $"Rendering threw {ex.GetType().Name}: {ex.Message}"));
                continue;
            }

            if (actual == null)
            {
                failures.Add(new(testCase.TemplateName, "The engine rendered nothing."));
                continue;
            }

            var difference = FirstDifference(testCase.Expected, actual);
            if (difference != null)
            {
                failures.Add(new(testCase.TemplateName, difference));
            }
        }

        return failures;
    }

    private static string? FirstDifference(string expected, string actual)
    {
        var want = Lines(expected);
        var got = Lines(actual);
        for (var i = 0; i < global::System.Math.Max(want.Length, got.Length); i++)
        {
            var w = i < want.Length ? want[i] : "<end>";
            var g = i < got.Length ? got[i] : "<end>";
            if (!string.Equals(w, g, StringComparison.Ordinal))
            {
                return $"Line {i + 1} differs. Expected '{w}', got '{g}'.";
            }
        }

        return null;
    }

    private static string[] Lines(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n').Split('\n');
}
