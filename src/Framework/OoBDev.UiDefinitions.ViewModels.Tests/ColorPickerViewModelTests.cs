using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary>The colour dialog state.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ColorPickerViewModelTests
{
    [TestMethod]
    public void Constructor_ValidColour_ReadsChannels()
    {
        // Test
        var model = new ColorPickerViewModel("255, 128,0");

        // Assert
        Assert.AreEqual(255, model.Red);
        Assert.AreEqual(128, model.Green);
        Assert.AreEqual(0, model.Blue);
        Assert.AreEqual("#FF8000", model.Preview);
        Assert.AreEqual("255,128,0", model.Result);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("1,2")]
    [DataRow("1,2,300")]
    [DataRow("a,b,c")]
    public void Constructor_Invalid_StartsBlack(string? text)
    {
        // Test
        var model = new ColorPickerViewModel(text);

        // Assert
        Assert.AreEqual("0,0,0", model.Result);
    }

    [TestMethod]
    public void Channel_OutOfRange_IsClampedAndNotifies()
    {
        // Stage
        var model = new ColorPickerViewModel(null);
        var raised = new List<string?>();
        model.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        // Test
        model.Red = 999;
        model.Green = -5;
        model.Blue = 7;

        // Assert
        Assert.AreEqual("255,0,7", model.Result);
        CollectionAssert.Contains(raised, nameof(ColorPickerViewModel.Preview));
    }

    [TestMethod]
    public void Commands_AcceptAndCancel_RaiseCloseRequested()
    {
        // Stage
        var model = new ColorPickerViewModel(null);
        var closed = new List<bool>();
        model.CloseRequested += (_, accepted) => closed.Add(accepted);

        // Test
        model.Accept.Execute(null);
        model.Cancel.Execute(null);

        // Assert
        CollectionAssert.AreEqual(new[] { true, false }, closed);
    }
}
