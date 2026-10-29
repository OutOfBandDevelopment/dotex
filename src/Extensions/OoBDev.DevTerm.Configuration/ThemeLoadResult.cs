using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>The outcome of loading one user theme file: the theme when it's valid, and every problem found either way.</summary>
/// <param name="Theme">The loaded theme, or <see langword="null"/> if the file was rejected.</param>
/// <param name="Errors">Why the file was rejected - empty when <paramref name="Theme"/> is set.</param>
/// <param name="Warnings">Problems that don't reject the file (low contrast) - worth showing, not fatal.</param>
public sealed record ThemeLoadResult(DevTermTheme? Theme, IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings);
