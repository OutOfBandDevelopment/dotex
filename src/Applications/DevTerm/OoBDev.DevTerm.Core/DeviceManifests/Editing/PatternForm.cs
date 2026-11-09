using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>One response pattern, plus an editor-only sample line to try it on.</summary>
[FormSection("Pattern", Order = 0, Label = "")]
[FormSection("Try it", Order = 1)]
public sealed class PatternForm : EditorForm
{
    private string _sample = string.Empty;

    public PatternForm(ResponsePattern pattern, Action edited)
        : base(edited)
    {
        Pattern = pattern;
    }

    public ResponsePattern Pattern { get; }

    [Category("Pattern")]
    [DisplayName("Name")]
    [Description("The value id the match publishes (its first capture group, or the whole match).")]
    [FormField(Order = 0)]
    public string Name
    {
        get => Pattern.Name;
        set
        {
            Pattern.Name = value;
            Changed();
            Notify(nameof(SampleResult));
        }
    }

    [Category("Pattern")]
    [DisplayName("Regex")]
    [Description("Tested against every complete reply line; each named group (?<id>...) publishes its own value.")]
    [FormField(Order = 1)]
    public string Match
    {
        get => Pattern.Match;
        set
        {
            Pattern.Match = value;
            Changed();
            Notify(nameof(SampleResult));
        }
    }

    [Category("Pattern")]
    [DisplayName("Example")]
    [Description("An example reply line this regex matches. Previews and the expression picker use the values in it.")]
    [FormField(Order = 2)]
    public string Example
    {
        get => Pattern.Example ?? string.Empty;
        set
        {
            Pattern.Example = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Try it")]
    [DisplayName("Sample line")]
    [Description("Not saved: a reply line to test the regex on.")]
    [FormField(Order = 0)]
    public string SampleLine
    {
        get => _sample;
        set
        {
            _sample = value;
            OnSampleChanged();
        }
    }

    [Category("Try it")]
    [DisplayName("Publishes")]
    [FormField(Order = 1, Kind = FormFieldKind.Indicator)]
    public string SampleResult
    {
        get
        {
            if (_sample.Length == 0)
            {
                return "(type a sample line)";
            }

            try
            {
                var match = new Regex(Pattern.Match, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250)).Match(_sample);
                if (!match.Success)
                {
                    return "no match";
                }

                var values = new List<string> { $"{Pattern.Name}={(match.Groups.Count > 1 ? match.Groups[1].Value : match.Value)}" };
                values.AddRange(match.Groups.Cast<Group>().Where(g => g.Success && !int.TryParse(g.Name, global::System.Globalization.CultureInfo.InvariantCulture, out _)).Select(g => $"{g.Name}={g.Value}"));
                return string.Join("  ", values);
            }
            catch (ArgumentException ex)
            {
                return "regex error: " + ex.Message;
            }
            catch (RegexMatchTimeoutException)
            {
                return "regex timed out";
            }
        }
    }

    // Editor-only: re-evaluates the sample without marking the manifest edited.
    private void OnSampleChanged()
    {
        Notify(nameof(SampleLine));
        Notify(nameof(SampleResult));
    }
}
