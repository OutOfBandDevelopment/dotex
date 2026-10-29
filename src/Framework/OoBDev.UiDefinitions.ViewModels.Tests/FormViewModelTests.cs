using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>The form view models and their commands over a <see cref="FormBinding"/>.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class FormViewModelTests
{
    private sealed class Model : INotifyPropertyChanged
    {
        private bool _advanced;
        private string _name = "a";
        private string _mode = "Low";
        private string _tags = "x";
        private int _count = 3;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool Advanced { get => _advanced; set => Set(ref _advanced, value); }

        public string Name { get => _name; set => Set(ref _name, value); }

        public string Mode { get => _mode; set => Set(ref _mode, value); }

        public string Tags { get => _tags; set => Set(ref _tags, value); }

        public int Count { get => _count; set => Set(ref _count, value); }

        public string ReadOnlyValue => "fixed";

        public RunCommand Run { get; } = new();

        private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    private sealed class RunCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public int Runs { get; private set; }

        public bool Allowed { get; set; } = true;

        public bool CanExecute(object? parameter) => Allowed;

        public void Execute(object? parameter) => Runs++;
    }

    private static UiDefinition Definition() => new()
    {
        Name = "test",
        Sections =
        [
            new UiSection
            {
                Label = "Main",
                Controls =
                [
                    new ToggleControl { Id = "Advanced", Label = "Advanced" },
                    new TextFieldControl { Id = "Name", Label = "Name", VisibleWhen = new UiCondition { Id = "Advanced" } },
                    new NumericControl { Id = "Count", Label = "Count", Minimum = 1, Maximum = 5 },
                    new ChoiceControl { Id = "Mode", Label = "Mode", Options = ["Low", "High"] },
                    new ChoiceControl { Id = "Tags", Label = "Tags", Options = ["x", "y", "z"], Style = ChoiceStyle.CheckList },
                    new TextFieldControl { Id = "ReadOnlyValue", Label = "Fixed" },
                    new ButtonControl { Id = "Run", Label = "Run", ConfirmMessage = "Sure?" },
                ],
            },
            new UiSection
            {
                Label = "Hidden",
                Controls = [new TextFieldControl { Id = "Name", Label = "Again", VisibleWhen = new UiCondition { Id = "Advanced" } }],
            },
        ],
    };

    private static (FormViewModel Form, Model Model) Create()
    {
        var model = new Model();
        return (new FormViewModel(Definition(), new FormBinding(model)), model);
    }

    [TestMethod]
    public void Constructor_Definition_BuildsOneRowPerControl()
    {
        // Test
        var (form, _) = Create();

        // Assert
        Assert.AreEqual(2, form.Sections.Count);
        Assert.AreEqual(8, form.Rows.Count());
        Assert.IsInstanceOfType<ToggleViewModel>(form.Row("Advanced"));
        Assert.IsInstanceOfType<ButtonViewModel>(form.Row("Run"));
        Assert.IsNull(form.Row("Missing"));
    }

    [TestMethod]
    public void Toggle_ParameterAndFlip_WritesModel()
    {
        // Stage
        var (form, model) = Create();
        var toggle = (ToggleViewModel)form.Row("Advanced")!;

        // Test
        toggle.Toggle.Execute(null);
        var afterFlip = model.Advanced;
        toggle.Toggle.Execute(false);

        // Assert
        Assert.IsTrue(afterFlip);
        Assert.IsFalse(model.Advanced);
        Assert.IsFalse(toggle.IsChecked);
    }

    [TestMethod]
    public void Condition_ModelChanges_ShowsAndHidesDependentRow()
    {
        // Stage
        var (form, model) = Create();
        var name = form.Row("Name")!;
        var hiddenBefore = !name.IsVisible;
        var sectionHidden = !form.Sections[1].HasVisibleRows;

        // Test
        model.Advanced = true;

        // Assert
        Assert.IsTrue(hiddenBefore);
        Assert.IsTrue(sectionHidden);
        Assert.IsTrue(name.IsVisible);
        Assert.IsTrue(form.Sections[1].HasVisibleRows);
    }

    [TestMethod]
    public void Text_ValidAndInvalid_WritesOrReportsError()
    {
        // Stage
        var (form, model) = Create();
        var count = (TextViewModel)form.Row("Count")!;

        // Test
        count.Text = "4";
        var good = (model.Count, count.Error);
        count.Text = "abc";

        // Assert
        Assert.AreEqual((4, (string?)null), good);
        Assert.IsNotNull(count.Error);
        Assert.AreEqual(4, model.Count);
    }

    [TestMethod]
    public void Text_ModelChanged_RefreshesTextAndClearsError()
    {
        // Stage
        var (form, model) = Create();
        var count = (TextViewModel)form.Row("Count")!;
        count.Text = "abc";

        // Test
        model.Count = 2;

        // Assert
        Assert.AreEqual("2", count.Text);
        Assert.IsNull(count.Error);
    }

    [TestMethod]
    public void Row_ReadOnlyProperty_IsDisabled()
    {
        var (form, _) = Create();

        Assert.IsFalse(form.Row("ReadOnlyValue")!.IsEnabled);
    }

    [TestMethod]
    public void Row_EnabledConditionNotMet_ReportsDisabledReason()
    {
        // Stage
        var model = new Model();
        var control = new TextFieldControl
        {
            Id = "Name",
            Label = "Name",
            EnabledWhen = new UiCondition { Id = "Advanced" },
            DisabledReason = "Needs advanced",
        };
        var row = new TextViewModel(control, new FormBinding(model));

        // Test
        var before = (row.IsEnabled, row.DisabledReason);
        model.Advanced = true;
        row.Refresh();

        // Assert
        Assert.AreEqual((false, (string?)"Needs advanced"), before);
        Assert.IsTrue(row.IsEnabled);
        Assert.IsNull(row.DisabledReason);
    }

    [TestMethod]
    public void Select_OptionOrUnknown_WritesOnlyKnownOptions()
    {
        // Stage
        var (form, model) = Create();
        var mode = (ChoiceViewModel)form.Row("Mode")!;

        // Test
        var unknown = mode.Select.CanExecute("Nope");
        mode.Select.Execute("Nope");
        mode.Select.Execute("High");

        // Assert
        Assert.IsFalse(unknown);
        Assert.AreEqual("High", model.Mode);
        Assert.AreEqual("High", mode.SelectedOption);
    }

    [TestMethod]
    public void ToggleOption_CheckAndUncheck_KeepsDefinitionOrder()
    {
        // Stage
        var (form, model) = Create();
        var tags = (ChoiceViewModel)form.Row("Tags")!;

        // Test
        tags.ToggleOption.Execute("z");
        tags.ToggleOption.Execute("y");
        var both = model.Tags;
        tags.ToggleOption.Execute("x");

        // Assert
        Assert.AreEqual("x,y,z", both);
        Assert.AreEqual("y,z", model.Tags);
        Assert.IsTrue(tags.IsChecked("Y"));
        Assert.IsFalse(tags.IsChecked("x"));
    }

    [TestMethod]
    public void Invoke_ModelCommand_RunsWhenAllowed()
    {
        // Stage
        var (form, model) = Create();
        var button = (ButtonViewModel)form.Row("Run")!;

        // Test
        button.Invoke.Execute(null);
        model.Run.Allowed = false;
        var allowed = button.Invoke.CanExecute(null);
        button.Invoke.Execute(null);

        // Assert
        Assert.AreEqual(1, model.Run.Runs);
        Assert.IsFalse(allowed);
        Assert.AreEqual("Sure?", button.ConfirmMessage);
    }

    [TestMethod]
    public void Invoke_NoSuchCommand_CannotExecute()
    {
        var button = new ButtonViewModel(new ButtonControl { Id = "Missing", Label = "M" }, new FormBinding(new Model()));

        Assert.IsFalse(button.Invoke.CanExecute(null));
    }

    [TestMethod]
    public void PropertyChanged_ValueChange_RaisesOnlyWhenDifferent()
    {
        // Stage
        var (form, model) = Create();
        var toggle = (ToggleViewModel)form.Row("Advanced")!;
        var raised = new List<string?>();
        toggle.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        // Test
        model.Advanced = true;
        model.Advanced = true;

        // Assert
        Assert.AreEqual(1, raised.Count(n => n == nameof(ToggleViewModel.IsChecked)));
    }

    [TestMethod]
    public void Dispose_ModelChanges_NoLongerRefreshes()
    {
        // Stage
        var (form, model) = Create();
        var toggle = (ToggleViewModel)form.Row("Advanced")!;

        // Test
        form.Dispose();
        model.Advanced = true;

        // Assert
        Assert.IsFalse(toggle.IsChecked);
    }

    [TestMethod]
    public void Create_LiveDisplayControl_Throws()
    {
        var binding = new FormBinding(new Model());

        Assert.ThrowsExactly<NotSupportedException>(() => ControlViewModelFactory.Create(new IndicatorControl { Id = "I", Label = "I" }, binding));
    }

    [TestMethod]
    public void Constructor_NullArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new FormViewModel(null!, new FormBinding(new Model())));
        Assert.ThrowsExactly<ArgumentNullException>(() => new ToggleCommand(null!, new ToggleControl { Id = "A", Label = "A" }));
    }

    [TestMethod]
    public void Generated_FormDefinition_BuildsViewModel()
    {
        // Stage
        var definition = FormDefinitionGenerator.Generate(new Model());

        // Test
        using var form = new FormViewModel(definition, new FormBinding(new Model()));

        // Assert
        Assert.IsTrue(form.Rows.Any());
    }
}
