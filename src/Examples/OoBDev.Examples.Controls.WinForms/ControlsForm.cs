using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OoBDev.Extensions.Windows.Forms;

namespace OoBDev.Examples.Controls.WinForms;

/// <summary>Shows the Windows Forms custom controls: a <see cref="ValidTextBox"/> per sample pattern, each with a status label fed by <see cref="ValidTextBox.ValidateChanged"/>, and one box whose <see cref="ValidTextBox.Filter"/> is typed in a second box.</summary>
public sealed class ControlsForm : Form
{
    /// <summary>The sample patterns: label, regular expression and a starting text.</summary>
    public static IReadOnlyList<(string Label, string Filter, string Text)> Samples { get; } =
    [
        ("Hex byte", "^[0-9A-Fa-f]{2}$", "7F"),
        ("Whole number", "^[0-9]+$", "42"),
        ("IPv4 address", @"^(\d{1,3}\.){3}\d{1,3}$", "10.0.0"),
    ];

    private readonly TableLayoutPanel _grid = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 3,
        Padding = new Padding(12),
        AutoSize = true,
    };

    /// <summary>The sample boxes, in <see cref="Samples"/> order, then the box that uses the typed filter.</summary>
    public IReadOnlyList<ValidTextBox> Boxes { get; }

    /// <summary>The box that edits the filter of the last <see cref="Boxes"/> entry.</summary>
    public TextBox FilterBox { get; } = new() { Dock = DockStyle.Fill, Text = "^[a-z]+$" };

    /// <summary>Creates the form with one row per sample.</summary>
    public ControlsForm()
    {
        Text = "Windows Forms controls";
        ClientSize = new Size(520, 220);
        _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

        var boxes = new List<ValidTextBox>();
        foreach (var (label, filter, text) in Samples)
        {
            boxes.Add(AddRow(label, new ValidTextBox { Filter = filter, Text = text }));
        }

        _grid.Controls.Add(new Label { Text = "Filter (regex)", AutoSize = true, Anchor = AnchorStyles.Left });
        _grid.Controls.Add(FilterBox);
        _grid.Controls.Add(new Label());
        var typed = AddRow("Typed filter", new ValidTextBox { Filter = FilterBox.Text, Text = "abc" });
        FilterBox.TextChanged += (_, _) => TryApplyFilter(typed, FilterBox.Text);
        boxes.Add(typed);

        Boxes = boxes;
        Controls.Add(_grid);
    }

    private ValidTextBox AddRow(string label, ValidTextBox box)
    {
        var status = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Text = Describe(box.IsValid) };
        box.Dock = DockStyle.Fill;
        box.ValidateChanged += (_, e) => status.Text = Describe(e.IsValid);
        _grid.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
        _grid.Controls.Add(box);
        _grid.Controls.Add(status);
        return box;
    }

    private static void TryApplyFilter(ValidTextBox box, string pattern)
    {
        try
        {
            box.Filter = pattern;
        }
        catch (ArgumentException)
        {
            box.Filter = null;
        }
    }

    private static string Describe(bool valid) => valid ? "valid" : "invalid";
}
