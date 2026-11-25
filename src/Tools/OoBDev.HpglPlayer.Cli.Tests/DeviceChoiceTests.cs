using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.HpglPlayer.Cli.Tests;

[TestClass]
public sealed class DeviceChoiceTests
{
    private static readonly string[] _names = ["Speakers (Realtek)", "Scope Interface (USB Audio)", "Headphones"];

    [TestMethod]
    [TestCategory("Unit")]
    public void Pick_Number_IsTheListPosition() => Assert.AreEqual(2, DeviceChoice.Pick(_names, "2"));

    [TestMethod]
    [TestCategory("Unit")]
    public void Pick_NamePart_MatchesWithoutCase() => Assert.AreEqual(1, DeviceChoice.Pick(_names, "scope"));

    [TestMethod]
    [TestCategory("Unit")]
    public void Pick_NumberPastTheEnd_Throws() => Assert.Throws<ArgumentException>(() => DeviceChoice.Pick(_names, "3"));

    [TestMethod]
    [TestCategory("Unit")]
    public void Pick_UnknownName_Throws() => Assert.Throws<ArgumentException>(() => DeviceChoice.Pick(_names, "midi"));
}
