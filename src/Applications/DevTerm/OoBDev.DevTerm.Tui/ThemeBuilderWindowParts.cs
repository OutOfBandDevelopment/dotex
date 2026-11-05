using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text;
using OoBDev.DevTerm.Configuration;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>The controls a test needs to drive <see cref="ThemeBuilderMode"/> headlessly.</summary>
internal sealed class ThemeBuilderWindowParts
{
    public required Window Window { get; init; }

    public required ThemeBuilderState State { get; init; }

    public required TextField NameField { get; init; }

    public required OptionSelector PaletteSelector { get; init; }

    public required ListView RolesList { get; init; }

    public required Label WarningsLabel { get; init; }

    /// <summary>"n / total" beside the role list.</summary>
    public required Label PositionLabel { get; init; }

    public required Button EditButton { get; init; }

    public required Button ResetButton { get; init; }

    public required Button SaveButton { get; init; }

    public required Button CancelButton { get; init; }
}
