using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.WinForms;

/// <summary>
/// Renders a <see cref="FormViewModel"/> as Windows Forms controls: a group box per section and a row per control. Editors write
/// through the view model (commands for toggles, choices and buttons, properties for text and sliders), and each row follows
/// its view model for value, visibility, enabled state and error.
/// </summary>
public sealed class FormView : UserControl
{
    private readonly ToolTip _tips = new();
    private readonly ErrorProvider _errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly List<Action> _unhook = [];
    private readonly Dictionary<string, Control> _editors = new(StringComparer.Ordinal);
    private bool _updating;

    /// <summary>Builds the controls for <paramref name="model"/>.</summary>
    public FormView(FormViewModel model)
    {
        ViewModel = model ?? throw new ArgumentNullException(nameof(model));
        AutoScroll = true;
        var stack = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Padding = new Padding(8) };
        foreach (var section in model.Sections)
        {
            stack.Controls.Add(Section(section));
        }

        Controls.Add(stack);
    }

    /// <summary>The bound view model.</summary>
    public FormViewModel ViewModel { get; }

    /// <summary>Every editor by control id, for tests and for callers that need to focus one.</summary>
    public IReadOnlyDictionary<string, Control> Editors => _editors;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var undo in _unhook)
            {
                undo();
            }

            _tips.Dispose();
            _errors.Dispose();
        }

        base.Dispose(disposing);
    }

    private GroupBox Section(FormSectionViewModel section)
    {
        var box = new GroupBox { Text = section.Label ?? string.Empty, AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(6) };
        var rows = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1 };
        foreach (var row in section.Rows)
        {
            rows.Controls.Add(Row(row));
        }

        box.Controls.Add(rows);
        Follow(section, () => box.Visible = section.HasVisibleRows);
        return box;
    }

    private TableLayoutPanel Row(ControlViewModel row)
    {
        var panel = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Dock = DockStyle.Top };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.Controls.Add(new Label { Text = row is ButtonViewModel ? string.Empty : row.Label, AutoSize = true, Anchor = AnchorStyles.Left });
        var editor = Editor(row);
        _editors[row.Id] = editor;
        panel.Controls.Add(editor);
        if (row.Description is { Length: > 0 } help)
        {
            _tips.SetToolTip(editor, help);
        }

        Follow(row, () =>
        {
            panel.Visible = row.IsVisible;
            editor.Enabled = row.IsEnabled;
            _errors.SetError(editor, row.Error ?? string.Empty);
        });
        return panel;
    }

    private Control Editor(ControlViewModel row) => row switch
    {
        ToggleViewModel toggle => ToggleEditor(toggle),
        ChoiceViewModel choice => ChoiceEditor(choice),
        ButtonViewModel button => ButtonEditor(button),
        TextViewModel { IsSlider: true } slider => SliderEditor(slider),
        TextViewModel text => TextEditor(text),
        _ => new Label { Text = row.Label, AutoSize = true },
    };

    private CheckBox ToggleEditor(ToggleViewModel toggle)
    {
        var box = new CheckBox { AutoSize = true };
        box.CheckedChanged += (_, _) =>
        {
            if (!_updating && toggle.Toggle.CanExecute(null))
            {
                toggle.Toggle.Execute(null);
            }
        };
        Follow(toggle, () => box.Checked = toggle.IsChecked);
        return box;
    }

    private FlowLayoutPanel ChoiceEditor(ChoiceViewModel choice)
    {
        var flow = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        foreach (var option in choice.OptionItems)
        {
            ButtonBase button = choice.Style == ChoiceStyle.CheckList
                ? new CheckBox { Text = option.Text, AutoSize = true }
                : new RadioButton { Text = option.Text, AutoSize = true };
            button.Click += (_, _) =>
            {
                if (!_updating && option.Command.CanExecute(option.Text))
                {
                    option.Command.Execute(option.Text);
                }
            };
            Follow(option, () => SetChecked(button, option.IsSelected));
            flow.Controls.Add(button);
        }

        return flow;
    }

    private static void SetChecked(ButtonBase button, bool value)
    {
        if (button is CheckBox check)
        {
            check.Checked = value;
        }
        else if (button is RadioButton radio)
        {
            radio.Checked = value;
        }
    }

    private Button ButtonEditor(ButtonViewModel button)
    {
        var control = new Button { Text = button.Label, AutoSize = true, Anchor = AnchorStyles.Left };
        control.Click += (_, _) =>
        {
            if (button.Invoke.CanExecute(null))
            {
                button.Invoke.Execute(null);
            }
        };
        if (button.ToolTip is { Length: > 0 } tip)
        {
            _tips.SetToolTip(control, tip);
        }

        return control;
    }

    private TextBox TextEditor(TextViewModel text)
    {
        var box = new TextBox { Dock = DockStyle.Fill };
        box.TextChanged += (_, _) =>
        {
            if (!_updating)
            {
                text.Text = box.Text;
            }
        };
        Follow(text, () => box.Text = text.Text);
        return box;
    }

    private TrackBar SliderEditor(TextViewModel slider)
    {
        var step = slider.Step > 0 ? slider.Step : 1;
        var bar = new TrackBar
        {
            Minimum = ToTicks(slider.Minimum, step),
            Maximum = ToTicks(slider.Maximum, step),
            TickFrequency = 1,
            AutoSize = false,
            Height = 32,
            Dock = DockStyle.Fill,
        };
        bar.ValueChanged += (_, _) =>
        {
            if (!_updating)
            {
                slider.Value = bar.Value * step;
            }
        };
        Follow(slider, () => bar.Value = Math.Clamp(ToTicks(slider.Value, step), bar.Minimum, bar.Maximum));
        return bar;
    }

    private static int ToTicks(double value, double step) => (int)Math.Round(value / step, MidpointRounding.AwayFromZero);

    private void Follow(INotifyPropertyChanged source, Action apply)
    {
        void Apply()
        {
            var was = _updating;
            _updating = true;
            try
            {
                apply();
            }
            finally
            {
                _updating = was;
            }
        }

        void Handler(object? sender, PropertyChangedEventArgs e) => Apply();
        source.PropertyChanged += Handler;
        _unhook.Add(() => source.PropertyChanged -= Handler);
        Apply();
    }
}
