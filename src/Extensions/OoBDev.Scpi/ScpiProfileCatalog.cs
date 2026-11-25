using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OoBDev.Scpi;

/// <summary>
/// The loaded instrument profiles: the bundled set (embedded JSON) plus any <c>*.json</c> files in the directories passed to
/// <see cref="Create"/>. A file that fails to load is skipped and reported in <see cref="LoadErrors"/>.
/// </summary>
public sealed class ScpiProfileCatalog
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() },
    };

    private ScpiProfileCatalog(List<ScpiInstrumentProfile> profiles, List<string> loadErrors)
    {
        Profiles = profiles;
        LoadErrors = loadErrors;
    }

    /// <summary>The always-available baseline for an instrument without a profile; it is never auto-selected.</summary>
    public static ScpiInstrumentProfile Generic { get; } = new()
    {
        Name = "Generic (manual)",
        Terminator = "\n",
        Commands =
        [
            new ScpiCommandDefinition { Id = "idn", Label = "Identify", Category = "Common", Template = "*IDN?", IsQuery = true },
            new ScpiCommandDefinition { Id = "rst", Label = "Reset", Category = "Common", Template = "*RST" },
            new ScpiCommandDefinition { Id = "cls", Label = "Clear Status", Category = "Common", Template = "*CLS" },
            new ScpiCommandDefinition { Id = "opc", Label = "Operation Complete?", Category = "Common", Template = "*OPC?", IsQuery = true },
        ],
    };

    /// <summary>The bundled profiles first, then those from the directories, in file name order.</summary>
    public IReadOnlyList<ScpiInstrumentProfile> Profiles { get; }

    /// <summary>One entry per profile that failed to load.</summary>
    public IReadOnlyList<string> LoadErrors { get; }

    /// <summary>
    /// Loads the bundled profiles and the profile files in the given directories. A directory that does not exist is skipped.
    /// </summary>
    /// <param name="dropInDirectories">Extra directories of profile <c>*.json</c> files.</param>
    /// <returns>The catalog.</returns>
    public static ScpiProfileCatalog Create(IEnumerable<string>? dropInDirectories = null)
    {
        var profiles = new List<ScpiInstrumentProfile>();
        var errors = new List<string>();
        var ownTerminator = new HashSet<ScpiInstrumentProfile>();
        var sources = new Dictionary<ScpiInstrumentProfile, string>();

        var assembly = typeof(ScpiProfileCatalog).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(n => n.StartsWith("OoBDev.Scpi.Profiles.", StringComparison.Ordinal)).Order(StringComparer.Ordinal))
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            TryAdd(reader.ReadToEnd(), name["OoBDev.Scpi.Profiles.".Length..], profiles, errors, ownTerminator, sources);
        }

        foreach (var directory in dropInDirectories ?? [])
        {
            if (!Directory.Exists(directory)) continue;

            foreach (var file in Directory.EnumerateFiles(directory, "*.json").Order(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    TryAdd(File.ReadAllText(file), Path.GetFileName(file), profiles, errors, ownTerminator, sources);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }

        ResolveExtends(profiles, errors, ownTerminator, sources);
        return new ScpiProfileCatalog(profiles, errors);
    }

    private static void ResolveExtends(List<ScpiInstrumentProfile> profiles, List<string> errors, HashSet<ScpiInstrumentProfile> ownTerminator, Dictionary<ScpiInstrumentProfile, string> sources)
    {
        var done = new HashSet<ScpiInstrumentProfile>();
        var failed = new HashSet<ScpiInstrumentProfile>();

        bool Resolve(ScpiInstrumentProfile profile, Stack<ScpiInstrumentProfile> path)
        {
            if (done.Contains(profile)) return true;
            if (failed.Contains(profile)) return false;
            if (string.IsNullOrWhiteSpace(profile.Extends))
            {
                done.Add(profile);
                return true;
            }

            if (path.Contains(profile))
            {
                errors.Add($"{sources[profile]}: '{profile.Name}' extends itself through a cycle.");
                failed.Add(profile);
                return false;
            }

            var baseProfile = string.Equals(profile.Extends, Generic.Name, StringComparison.OrdinalIgnoreCase)
                ? Generic
                : profiles.FirstOrDefault(p => !ReferenceEquals(p, profile) && string.Equals(p.Name, profile.Extends, StringComparison.OrdinalIgnoreCase));
            if (baseProfile is null)
            {
                errors.Add($"{sources[profile]}: '{profile.Name}' extends '{profile.Extends}', which is not a loaded profile.");
                failed.Add(profile);
                return false;
            }

            path.Push(profile);
            var baseResolved = ReferenceEquals(baseProfile, Generic) || Resolve(baseProfile, path);
            path.Pop();
            if (!baseResolved)
            {
                errors.Add($"{sources[profile]}: '{profile.Name}' extends '{profile.Extends}', which failed to load.");
                failed.Add(profile);
                return false;
            }

            var own = profile.Commands.ToList();
            var merged = baseProfile.Commands.Select(c => own.FirstOrDefault(o => o.Id == c.Id) ?? c).ToList();
            merged.AddRange(own.Where(o => baseProfile.Commands.All(c => c.Id != o.Id)));
            profile.Commands = merged;
            if (!ownTerminator.Contains(profile)) profile.Terminator = baseProfile.Terminator;
            profile.Notes ??= baseProfile.Notes;
            done.Add(profile);
            return true;
        }

        foreach (var profile in profiles.ToList())
        {
            Resolve(profile, new Stack<ScpiInstrumentProfile>());
        }

        profiles.RemoveAll(failed.Contains);
    }

    /// <summary>
    /// Finds the first profile whose <see cref="ScpiInstrumentProfile.IdnPattern"/> matches an <c>*IDN?</c> reply. SCPI has no
    /// query that lists supported commands, so this is as far as detection goes.
    /// </summary>
    /// <param name="idnReply">The <c>*IDN?</c> reply.</param>
    /// <returns>The profile, or <see langword="null"/> when none matches (use <see cref="Generic"/>).</returns>
    public ScpiInstrumentProfile? TryMatchByIdn(string idnReply)
    {
        ArgumentNullException.ThrowIfNull(idnReply);

        foreach (var profile in Profiles)
        {
            if (string.IsNullOrEmpty(profile.IdnPattern)) continue;

            try
            {
                if (Regex.IsMatch(idnReply, profile.IdnPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250)))
                    return profile;
            }
            catch (RegexMatchTimeoutException)
            {
                // a pattern with catastrophic backtracking counts as no match; the rest are still checked
            }
            catch (ArgumentException)
            {
                // an invalid pattern in a drop-in file counts as no match
            }
        }

        return null;
    }

    private static void TryAdd(string json, string source, List<ScpiInstrumentProfile> profiles, List<string> errors, HashSet<ScpiInstrumentProfile> ownTerminator, Dictionary<ScpiInstrumentProfile, string> sources)
    {
        try
        {
            var profile = JsonSerializer.Deserialize<ScpiInstrumentProfile>(json, SerializerOptions);
            if (profile is null) return;
            using (var document = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip }))
            {
                if (document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.EnumerateObject().Any(p => string.Equals(p.Name, nameof(ScpiInstrumentProfile.Terminator), StringComparison.OrdinalIgnoreCase)))
                {
                    ownTerminator.Add(profile);
                }
            }

            sources[profile] = source;

            // Math.Clamp throws when the minimum exceeds the maximum, so reject the profile here, not at invoke time.
            var bad = profile.Commands
                .SelectMany(command => command.Parameters)
                .FirstOrDefault(p => p is { Kind: ScpiParameterKind.Numeric, Minimum: { } min, Maximum: { } max } && min > max);
            if (bad is not null)
            {
                errors.Add($"{source}: parameter '{bad.Name}' has Minimum ({bad.Minimum}) greater than Maximum ({bad.Maximum}).");
                return;
            }

            profiles.Add(profile);
        }
        catch (JsonException ex)
        {
            errors.Add($"{source}: {ex.Message}");
        }
    }
}
