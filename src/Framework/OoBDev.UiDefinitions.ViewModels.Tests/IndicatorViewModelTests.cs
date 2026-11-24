using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>A read-only form row shows the model's text and follows it.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class IndicatorViewModelTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class Model : INotifyPropertyChanged
    {
        private string _status = "ready";

        public event PropertyChangedEventHandler? PropertyChanged;

        [FormField(Warning = true)]
        public string Status => _status;

        public void Change(string value)
        {
            _status = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    [TestMethod]
    public void Row_ReadOnlyProperty_ShowsTheTextAsAWarning_AndFollowsTheModel()
    {
        // Stage
        var model = new Model();
        using var form = new FormViewModel(FormDefinitionGenerator.Generate(model, "t"), new FormBinding(model));
        var row = (IndicatorViewModel)form.Row(nameof(Model.Status))!;
        TestContext.WriteLine($"initial text '{row.Text}'");

        // Test
        model.Change("gone");

        // Assert
        Assert.IsTrue(row.IsWarning);
        Assert.AreEqual("gone", row.Text);
    }
}
