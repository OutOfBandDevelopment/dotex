using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace OoBDev.LanC.Tests;

[TestClass]
public class LanCTableTests
{
    [TestMethod]
    [TestCategory("Unit")]
    [DataRow(typeof(LanCSubCommand))]
    [DataRow(typeof(LanCCameraSpecialCommand))]
    [DataRow(typeof(LanCCameraVtrCommand))]
    [DataRow(typeof(LanCStatusCode))]
    public void Table_Members_HaveUniqueByteValues(Type table)
    {
        // Stage
        var values = Enum.GetValues(table).Cast<object>().Select(v => Convert.ToByte(v, global::System.Globalization.CultureInfo.InvariantCulture)).ToList();

        // Assert
        Assert.AreEqual(values.Count, values.Distinct().Count(), $"{table.Name} has two members with the same byte");
        Assert.AreEqual(typeof(byte), Enum.GetUnderlyingType(table));
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow(LanCSubCommand.CameraVtrNormal, (byte)0x18)]
    [DataRow(LanCSubCommand.CameraSpecial, (byte)0x28)]
    [DataRow(LanCSubCommand.VtrSpecial, (byte)0x38)]
    [DataRow(LanCSubCommand.StillNormal, (byte)0x1E)]
    public void SubCommand_Value_MatchesTheLancByteZeroTable(LanCSubCommand command, byte expected)
    {
        Assert.AreEqual(expected, (byte)command);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow(LanCCameraVtrCommand.Stop, (byte)0x30)]
    [DataRow(LanCCameraVtrCommand.Play, (byte)0x34)]
    [DataRow(LanCCameraVtrCommand.FastForward, (byte)0x38)]
    [DataRow(LanCCameraVtrCommand.Rewind, (byte)0x36)]
    [DataRow(LanCCameraVtrCommand.PowerOff, (byte)0x5E)]
    public void CameraVtrCommand_Key_MatchesTheTable(LanCCameraVtrCommand command, byte expected)
    {
        Assert.AreEqual(expected, (byte)command);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow(LanCCameraSpecialCommand.ZoomTelephotoSlowest, (byte)0x00)]
    [DataRow(LanCCameraSpecialCommand.ZoomTelephotoFastest, (byte)0x0E)]
    [DataRow(LanCCameraSpecialCommand.ZoomWideSlowest, (byte)0x10)]
    [DataRow(LanCCameraSpecialCommand.ZoomWideFastest, (byte)0x1E)]
    [DataRow(LanCCameraSpecialCommand.RecordStart, (byte)0x27)]
    [DataRow(LanCCameraSpecialCommand.RecordStop, (byte)0x29)]
    public void CameraSpecialCommand_Key_MatchesTheTable(LanCCameraSpecialCommand command, byte expected)
    {
        Assert.AreEqual(expected, (byte)command);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow(LanCStatusCode.Play, (byte)0x06)]
    [DataRow(LanCStatusCode.Record, (byte)0x04)]
    [DataRow(LanCStatusCode.Stop, (byte)0x02)]
    [DataRow(LanCStatusCode.LowBattery, (byte)0x32)]
    public void StatusCode_Value_MatchesTheTable(LanCStatusCode code, byte expected)
    {
        Assert.AreEqual(expected, (byte)code);
    }
}
