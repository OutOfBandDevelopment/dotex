using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.UiDefinitions.Tui;

/// <summary>
/// The TUI's generic <em>form</em> renderer: walks a <see cref="UiDefinition"/> (typically one
/// <see cref="FormDefinitionGenerator"/> made from an annotated model) and builds real Terminal.Gui
/// controls two-way bound to the model through a <see cref="FormBinding"/> — the settings-form sibling
/// of <see cref="ControlPanelMode"/>, which renders the same vocabulary as a <em>command</em> panel
/// (a control there sends on commit; a field here writes a property). Returns an embeddable
/// <see cref="View"/> rather than a window, so a host (the Connection Editor, the manifest editor)
/// places it among its own hand-built parts. See docs/design/UiDefinitions/ui-definitions.md's "Forms from one
/// definition".
/// </summary>
/// <remarks>
/// <para>
/// Layout: a labeled section starts with a <c>── Label ──</c> header line; within a section every
/// widget starts in one column (the section's longest label plus padding), a toggle's or button's
/// text standing in for its label. A section or row whose <see cref="UiCondition"/> doesn't hold is hidden and everything
/// below moves up (absolute rows, recomputed on every model change) — the hand-built editor this
/// replaced left a gap where a hidden transport's fields had been.
/// </para>
/// <para>
/// Widgets, with the installed Terminal.Gui v2.5.0 having no combo box: a choice that fits on one
/// line is an <see cref="OptionSelector"/>; one too wide for the terminal wraps as radio-style check
/// boxes over up to three lines; a longer list is a text field plus a <c>Pick...</c> button that opens
/// a list (the "type it or pick it" pattern the editor already used for its SCPI profile). A
/// <see cref="ChoiceStyle.CheckList"/> is a row of check boxes, wrapped the same way. A host can
/// replace any control's widget (<see cref="TuiFormOptions.CustomWidgets"/>) — the editor's
/// detected-device pickers — while the form still owns its label, alignment and visibility.
/// </para>
/// </remarks>
public static class FormRenderer
{
    internal const int MaxWrappedChoiceRows = 3;

    /// <summary>How far a section's rows sit in from its header.</summary>
    internal const int Indent = 2;

    /// <summary>The Terminal.Gui scheme warning text is drawn with - the theme's <c>Error</c> role (see <see cref="TuiTheme"/>), so a theme switch recolors it.</summary>
    public const string ErrorSchemeName = "Error";

    public static TuiFormParts Build(IApplication app, UiDefinition definition, FormBinding binding, TuiFormOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(binding);
        options ??= new TuiFormOptions();

        var available = options.AvailableWidth ?? Math.Max((app.Screen.Width > 0 ? app.Screen.Width : 80) - 4, 40);
        var parts = new TuiFormParts(binding)
        {
            Root = new View { X = 0, Y = 0, Width = Dim.Fill(), Height = 1, CanFocus = true },
        };

        foreach (var section in definition.Sections)
        {
            // Every widget in a section starts in the same column: its longest "Label:" plus a
            // space (per section, like the control panels, so one long label elsewhere doesn't
            // squeeze every other section's widgets to the right).
            var labelWidth = section.Controls.Where(HasRowLabel).Select(c => c.Label.Length + 1).DefaultIfEmpty(0).Max();
            var column = Indent + (labelWidth > 0 ? labelWidth + 1 : 0);
            var block = new SectionRows(section);
            if (!string.IsNullOrWhiteSpace(section.Label))
            {
                View header;
                if (options.CollapsibleSections)
                {
                    // A focusable header (Enter/click toggles), like the control panel's; remembered per definition.
                    block.Expanded = SectionExpansionState.IsExpanded(definition.Name, section.Label);
                    var button = new Button
                    {
                        X = 0,
                        Text = block.HeaderText,
                        NoDecorations = true,
                        NoPadding = true,
                        ShadowStyle = ShadowStyles.None,
                        HotKeySpecifier = (Rune)0xFFFF,
                    };
                    var captured = block;
                    button.Accepting += (_, e) =>
                    {
                        captured.Expanded = !captured.Expanded;
                        SectionExpansionState.Set(definition.Name, section.Label, captured.Expanded);
                        parts.Relayout();
                        e.Handled = true;
                    };
                    header = button;
                }
                else
                {
                    header = new Label { X = 0, Text = block.HeaderText };
                }

                parts.Root.Add(header);
                parts.SectionHeaderLabels[section.Label] = header;
                block.Header = header;
            }

            foreach (var control in section.Controls)
            {
                var row = BuildRow(app, parts, control, Indent, column, available, options);
                row.Label = parts.RowLabels.GetValueOrDefault(control.Id);
                row.CaptureColumnOffsets(column);
                foreach (var item in row.Items)
                {
                    parts.Root.Add(item.View);
                }

                block.Rows.Add(row);
            }

            parts.Sections.Add(block);
        }

        parts.RefreshAll();
        binding.Changed += (_, name) => parts.OnModelChanged(name);
        return parts;
    }

    /// <summary>
    /// A small modal "pick one" list — the stand-in for the combo box Terminal.Gui doesn't have.
    /// Returns the picked <em>index</em>, not the text: two entries can show identical text (three
    /// attached K8055 boards sharing a VID/PID with no serial), and indexing the caller's own list is
    /// unambiguous where a text match isn't.
    /// </summary>
    public static int? PickFromList(IApplication app, string title, IReadOnlyList<string> items, string emptyMessage = "Nothing was detected.")
    {
        if (items.Count == 0)
        {
            MessageBox.Query(app, "dev-term", emptyMessage, ["OK"]);
            return null;
        }

        int? picked = null;
        var filtered = Enumerable.Range(0, items.Count).ToList();
        var dialog = new Dialog { Title = title, Width = ListDialogWidth(app, items), Height = Math.Min(items.Count + 6, 20) };
        var filterField = new TextField { X = 0, Y = 0, Width = Dim.Fill() };
        var listView = new ListView { X = 0, Y = Pos.Bottom(filterField), Width = Dim.Fill(), Height = Dim.Fill(2) };
        listView.SetSource(new ObservableCollection<string>(items));

        void ApplyFilter()
        {
            filtered = FilterIndices(items, filterField.Text);
            listView.SetSource(new ObservableCollection<string>(filtered.Select(i => items[i])));
            if (filtered.Count > 0)
            {
                listView.SelectedItem = 0;
            }
        }

        void Choose()
        {
            if (listView.SelectedItem is int index && index >= 0 && index < filtered.Count)
            {
                picked = filtered[index];
            }

            app.RequestStop();
        }

        filterField.TextChanged += (_, _) => ApplyFilter();
        filterField.KeyDown += (_, key) =>
        {
            if (key == Key.CursorUp)
            {
                key.Handled = true;
                var current = listView.SelectedItem is int idx ? idx : 0;
                if (filtered.Count > 0)
                {
                    listView.SelectedItem = Math.Max(0, current - 1);
                }

                return;
            }

            if (key == Key.CursorDown)
            {
                key.Handled = true;
                var current = listView.SelectedItem is int idx ? idx : 0;
                if (filtered.Count > 0)
                {
                    listView.SelectedItem = Math.Min(filtered.Count - 1, current + 1);
                }
            }
        };

        listView.Accepting += (_, e) =>
        {
            e.Handled = true;
            Choose();
        };
        var selectButton = new Button { X = 0, Y = Pos.Bottom(listView), Text = "Select", IsDefault = true };
        selectButton.Accepting += (_, e) =>
        {
            e.Handled = true;
            Choose();
        };
        var cancelButton = new Button { X = Pos.Right(selectButton) + 1, Y = Pos.Top(selectButton), Text = "Cancel" };
        cancelButton.Accepting += (_, e) =>
        {
            e.Handled = true;
            app.RequestStop();
        };
        dialog.Add(filterField, listView, selectButton, cancelButton);
        app.Run(dialog);
        return picked;
    }

    /// <summary>
    /// <see cref="PickFromList"/>'s filter box, as a case-insensitive substring match: the original
    /// indices into <paramref name="items"/> whose text contains <paramref name="filterText"/>, in
    /// their original order, or every index (also in original order) when the filter is empty.
    /// </summary>
    internal static List<int> FilterIndices(IReadOnlyList<string> items, string filterText) =>
        string.IsNullOrEmpty(filterText)
            ? [.. Enumerable.Range(0, items.Count)]
            : [.. Enumerable.Range(0, items.Count).Where(i => items[i].Contains(filterText, StringComparison.OrdinalIgnoreCase))];

    /// <summary>
    /// A list picker's width: wide enough for its longest item (the SCPI profile names ran past a
    /// fixed 60 columns and were cut off) but never wider than the screen, and at least 60. Its list
    /// leaves the last two rows to the Select/Cancel row and its shadow, which a one-row gap cut off.
    /// </summary>
    public static int ListDialogWidth(IApplication app, IReadOnlyList<string> items)
    {
        var screen = app.Screen.Width > 0 ? app.Screen.Width : 80;
        var widest = items.Count == 0 ? 0 : items.Max(i => i.Length);
        return Math.Clamp(widest + 4, 60, Math.Max(screen - 4, 60));
    }

    /// <summary>Greedy placement of items <paramref name="widths"/> wide, <paramref name="gap"/> apart, into lines of at most <paramref name="width"/> columns: each item's (column offset, line).</summary>
    internal static List<(int X, int Line)> Wrap(IReadOnlyList<int> widths, int width, int gap)
    {
        var placed = new List<(int X, int Line)>(widths.Count);
        var x = 0;
        var line = 0;
        foreach (var w in widths)
        {
            if (x > 0 && x + w > width)
            {
                x = 0;
                line++;
            }

            placed.Add((x, line));
            x += w + gap;
        }

        return placed;
    }

    private static bool HasRowLabel(UiControl control) =>
        control is not ToggleControl and not ButtonControl && !string.IsNullOrEmpty(control.Label);

    private static FormRow BuildRow(IApplication app, TuiFormParts parts, UiControl control, int indent, int column, int available, TuiFormOptions options)
    {
        var binding = parts.Binding;
        var row = new FormRow(control);
        if (HasRowLabel(control))
        {
            var label = new Label { X = indent, Text = control.Label + ":" };
            row.Add(label);
            parts.RowLabels[control.Id] = label;
        }

        var width = Math.Max(available - column, 10);
        if (options.CustomWidgets.TryGetValue(control.Id, out var custom))
        {
            var widget = custom(control);
            widget.View.X = column;
            row.Add(widget.View);
            row.Height = Math.Max(widget.Rows, 1);
            parts.ControlViews[control.Id] = widget.View;
            return row;
        }

        switch (control)
        {
            case ToggleControl:
                {
                    var checkBox = new CheckBox { X = column, Text = control.Label };
                    checkBox.ValueChanged += (_, _) => parts.Push(() => binding.SetBool(control.Id, checkBox.Value == CheckState.Checked));
                    row.Add(checkBox);
                    row.Refresh = () => checkBox.Value = binding.GetBool(control.Id) ? CheckState.Checked : CheckState.UnChecked;
                    parts.ControlViews[control.Id] = checkBox;
                    break;
                }

            case ChoiceControl { Style: ChoiceStyle.CheckList } choice:
                {
                    var boxes = choice.Options.Select(option => new CheckBox { Text = option }).ToList();
                    var placed = Wrap([.. choice.Options.Select(o => o.Length + 2)], width, 1);
                    for (var i = 0; i < boxes.Count; i++)
                    {
                        boxes[i].X = column + placed[i].X;
                        row.Add(boxes[i], placed[i].Line);
                        boxes[i].ValueChanged += (_, _) => parts.Push(() =>
                            binding.SetSelection(control.Id, choice.Options.Where((_, index) => boxes[index].Value == CheckState.Checked)));
                    }

                    row.Height = placed.Count == 0 ? 1 : placed[^1].Line + 1;
                    row.Refresh = () =>
                    {
                        var selected = binding.GetSelection(control.Id);
                        for (var i = 0; i < boxes.Count; i++)
                        {
                            boxes[i].Value = selected.Contains(choice.Options[i], StringComparer.OrdinalIgnoreCase) ? CheckState.Checked : CheckState.UnChecked;
                        }
                    };
                    parts.CheckLists[control.Id] = boxes;
                    parts.ControlViews[control.Id] = boxes.Count > 0 ? boxes[0] : new View();
                    break;
                }

            case ChoiceControl choice:
                BuildChoice(app, parts, row, choice, column, width);
                break;

            case IndicatorControl indicator:
                {
                    var label = new Label { X = column, Text = indicator.DefaultValue ?? string.Empty };
                    if (indicator.Style == IndicatorStyle.Warning)
                    {
                        // The theme's Error color, like WPF's form renderer and the inline "! ..."
                        // messages below - a "(not found ...)" hint used to look like plain help text.
                        label.SchemeName = ErrorSchemeName;
                    }

                    row.Add(label);
                    row.Refresh = () =>
                    {
                        var text = binding.Has(control.Id) ? binding.GetText(control.Id) : indicator.DefaultValue ?? string.Empty;
                        var lines = TuiTextWrap.WordWrap(text, width);
                        label.Text = string.Join('\n', lines);
                        label.Width = Math.Max(lines.Max(l => l.Length), 1);
                        label.Height = lines.Count;
                        row.Height = lines.Count;
                    };
                    parts.ControlViews[control.Id] = label;
                    break;
                }

            case ButtonControl button:
                {
                    var view = new Button { X = column, Text = control.Label };
                    view.Accepting += (_, e) =>
                    {
                        e.Handled = true;
                        if (options.Actions.TryGetValue(control.Id, out var action))
                        {
                            action();
                        }
                        else
                        {
                            binding.Invoke(button.CommandId ?? button.Id);
                        }
                    };
                    row.Add(view);
                    row.Height = 2; // the button's shadow gets its own line
                    parts.ControlViews[control.Id] = view;
                    break;
                }

            default:
                BuildTextField(app, parts, row, control, column, width, options);
                break;
        }

        return row;
    }

    private static void BuildTextField(IApplication app, TuiFormParts parts, FormRow row, UiControl control, int column, int width, TuiFormOptions options)
    {
        var binding = parts.Binding;
        var constraint = ValueValidator.ConstraintFor(control);
        var numeric = constraint is { Kind: not ValueKind.Text };
        var fieldWidth = numeric ? 10 : control is TextFieldControl { MaxLength: { } max } ? Math.Clamp(max + 1, 4, 40) : 30;

        // Never wider than the room left in the row: in a narrow form (the manifest editor's pane
        // at 80 columns) a 30-wide field ran past the pane's edge and was cut off.
        var picker = options.TextPickers.GetValueOrDefault(control.Id);
        fieldWidth = Math.Min(fieldWidth, Math.Max(width - (picker is null ? 0 : 13), 4));
        var field = new TextField { X = column, Width = fieldWidth };
        row.Add(field);
        var next = column + fieldWidth + 1;

        if (picker is not null)
        {
            // A "Pick..." button after the field opens the host's picker (the expression picker), which
            // returns the new text or null when cancelled.
            var pick = new Button { X = next, Text = "Pick...", ShadowStyle = ShadowStyles.None };
            pick.Accepting += (_, e) =>
            {
                e.Handled = true;
                if (picker(field.Text) is { } picked)
                {
                    field.Text = picked;
                }
            };
            row.Add(pick);
            row.Height = 2;
            parts.PickButtons[control.Id] = pick;
            next += 12;
        }

        if (control is NumericControl or SliderControl && RangeHint(control) is { } hintText)
        {
            var hint = new Label { X = next, Text = hintText };
            row.Add(hint);
            next += hintText.Length + 1;
        }

        // Always "shown" with the row; empty (so invisible) while the value is valid.
        var error = new Label { X = next, Text = string.Empty, SchemeName = ErrorSchemeName };
        row.Add(error);
        parts.ErrorLabels[control.Id] = error;

        void ShowResult(ValueValidationResult result) => error.Text = result.IsValid ? string.Empty : "! " + result.Error;

        field.TextChanged += (_, _) => parts.Push(() => ShowResult(binding.SetText(control, field.Text)));
        row.Refresh = () =>
        {
            var text = binding.GetText(control.Id);
            if (field.Text != text)
            {
                field.Text = text;
            }

            ShowResult(binding.Validate(control, text));
        };
        field.ReadOnly = binding.IsReadOnly(control.Id);
        parts.ControlViews[control.Id] = field;
    }

    private static string? RangeHint(UiControl control)
    {
        var (min, max, unit) = control switch
        {
            NumericControl n => (n.Minimum, n.Maximum, n.Unit),
            SliderControl s => (s.Minimum, s.Maximum, s.Unit),
            _ => (0d, 0d, null),
        };

        return min <= double.MinValue / 2 || max >= double.MaxValue / 2
            ? unit
            : string.Create(CultureInfo.InvariantCulture, $"[{min:0.#}-{max:0.#}]{unit}");
    }

    private static void BuildChoice(IApplication app, TuiFormParts parts, FormRow row, ChoiceControl choice, int column, int width)
    {
        var binding = parts.Binding;
        var options = choice.Options;
        var labels = options.Select(ChoiceLabel).ToList();
        var selectorWidth = labels.Sum(o => o.Length + 2) + (2 * Math.Max(options.Count - 1, 0));

        if (selectorWidth <= width)
        {
            var selector = new OptionSelector
            {
                X = column,
                Orientation = Orientation.Horizontal,
                HorizontalSpace = 2,
                Labels = labels,

                // Options are data, not menu text — an '_' in one isn't a hotkey marker.
                HotKeySpecifier = (Rune)0xFFFF,
            };
            selector.ValueChanged += (_, _) =>
            {
                if (selector.Value is int index && index >= 0 && index < options.Count)
                {
                    parts.Push(() => binding.SetText(choice, options[index]));
                }
            };
            row.Add(selector);
            row.Refresh = () =>
            {
                var index = IndexOf(options, binding.GetText(choice.Id));
                if (selector.Value != (index >= 0 ? index : null))
                {
                    selector.Value = index >= 0 ? index : null;
                }
            };
            parts.ControlViews[choice.Id] = selector;
            parts.Choices[choice.Id] = new TuiChoice(options, selector, () => selector.Value is int i && i >= 0 && i < options.Count ? options[i] : null, value =>
            {
                var index = IndexOf(options, value);
                selector.Value = index >= 0 ? index : null;
            });
            return;
        }

        var placed = Wrap([.. labels.Select(o => o.Length + 2)], width, 2);
        if (placed.Count > 0 && placed[^1].Line < MaxWrappedChoiceRows)
        {
            var radios = labels.Select(label => new CheckBox { Text = label, RadioStyle = true, HotKeySpecifier = (Rune)0xFFFF }).ToList();
            for (var i = 0; i < radios.Count; i++)
            {
                var radio = radios[i];
                var option = options[i];
                radio.X = column + placed[i].X;
                row.Add(radio, placed[i].Line);
                radio.ValueChanged += (_, _) =>
                {
                    if (parts.IsRefreshing)
                    {
                        return;
                    }

                    if (radio.Value == CheckState.Checked)
                    {
                        parts.Push(() => binding.SetText(choice, option));
                    }

                    // Keep exactly the model's value checked (a radio can't be unchecked by itself).
                    parts.RefreshRow(row);
                };
            }

            row.Height = placed[^1].Line + 1;
            row.Refresh = () =>
            {
                var current = IndexOf(options, binding.GetText(choice.Id));
                for (var i = 0; i < radios.Count; i++)
                {
                    var check = i == current ? CheckState.Checked : CheckState.UnChecked;
                    if (radios[i].Value != check)
                    {
                        radios[i].Value = check;
                    }
                }
            };
            parts.CheckLists[choice.Id] = radios;
            parts.ControlViews[choice.Id] = radios[0];
            parts.Choices[choice.Id] = new TuiChoice(
                options,
                radios[0],
                () => radios.FindIndex(r => r.Value == CheckState.Checked) is var i and >= 0 ? options[i] : null,
                value =>
                {
                    if (IndexOf(options, value) is var i and >= 0)
                    {
                        radios[i].Value = CheckState.Checked;
                    }
                });
            return;
        }

        // Too many to show at once: type it, or pick it from the list.
        // Room for the "⟦ Pick... ⟧" button and its shadow (12 columns) plus a gap after the field.
        var field = new TextField { X = column, Width = Math.Min(30, Math.Max(width - 13, 10)) };
        var pick = new Button { X = Pos.Right(field) + 1, Text = "Pick..." };
        field.TextChanged += (_, _) => parts.Push(() => binding.SetText(choice, field.Text));
        pick.Accepting += (_, e) =>
        {
            e.Handled = true;
            if (PickFromList(app, choice.Label, options) is int index)
            {
                field.Text = options[index];
            }
        };
        row.Add(field);
        row.Add(pick);
        row.Height = 2;
        row.Refresh = () =>
        {
            var text = binding.GetText(choice.Id);
            if (field.Text != text)
            {
                field.Text = text;
            }
        };
        parts.ControlViews[choice.Id] = field;
        parts.PickButtons[choice.Id] = pick;
        parts.Choices[choice.Id] = new TuiChoice(options, field, () => field.Text, value => field.Text = value ?? string.Empty);
    }

    /// <summary>How a choice option is shown: an empty option (the "not set" choice, e.g. a manifest's transport hint) as "(none)" rather than a bare radio button with nothing beside it.</summary>
    internal static string ChoiceLabel(string option) => option.Length == 0 ? "(none)" : option;

    private static int IndexOf(IReadOnlyList<string> options, string? value)
    {
        for (var i = 0; i < options.Count; i++)
        {
            if (string.Equals(options[i], value, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}

