using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.TestUtilities.Tests;

[TestClass]
public class NumericAssertsTests
{
    public required TestContext TestContext { get; set; }

    #region AreSimilar - Double

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_ExactMatch()
    {
        // Stage
        var expected = 123.456;
        var actual = 123.456;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_WithinTolerance()
    {
        // Stage
        var expected = 123.456789012345;
        var actual = 123.456789012346; // Differs in last digit

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_BeyondTolerance()
    {
        // Stage
        var expected = 123.456;
        var actual = 123.457;

        // Test - Should throw
        Assert.Throws<AssertFailedException>(() => NumericAsserts.AreSimilar(expected, actual));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_LargeValues()
    {
        // Stage
        var expected = -59423282750552.782382789829384;
        var actual = -59423282750552.782382789829386;

        // Test & Assert - Real-world example from expression optimizer
        NumericAsserts.AreSimilar(expected, actual);
    }

    #endregion

    #region AreSimilar - Float

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Float_ExactMatch()
    {
        // Stage
        var expected = 123.456f;
        var actual = 123.456f;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Float_WithinTolerance()
    {
        // Stage
        var expected = 123.4567f;
        var actual = 123.45671f;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Float_BeyondTolerance()
    {
        // Stage
        var expected = 123.456f;
        var actual = 123.46f;

        // Test - Should throw
        Assert.Throws<AssertFailedException>(() => NumericAsserts.AreSimilar(expected, actual));
    }

    #endregion

    #region AreSimilar - Decimal

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Decimal_ExactMatch()
    {
        // Stage
        var expected = 123.456789012345678901234567890m;
        var actual = 123.456789012345678901234567890m;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Decimal_WithinTolerance()
    {
        // Stage
        var expected = 123.456789012345678901234567890m;
        var actual = 123.456789012345678901234567891m;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Decimal_BeyondTolerance()
    {
        // Stage
        var expected = 123.456m;
        var actual = 123.457m;

        // Test - Should throw
        Assert.Throws<AssertFailedException>(() => NumericAsserts.AreSimilar(expected, actual));
    }

    #endregion

    #region AreSimilar - Integer Types

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Int32_ExactMatch()
    {
        // Stage
        var expected = 12345;
        var actual = 12345;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Int32_NotEqual()
    {
        // Stage
        var expected = 12345;
        var actual = 12346;

        // Test - Should throw
        Assert.Throws<AssertFailedException>(() => NumericAsserts.AreSimilar(expected, actual));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Int64_ExactMatch()
    {
        // Stage
        var expected = 123456789012345;
        var actual = 123456789012345;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    #endregion

    #region AreSimilar with Custom Tolerance

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_CustomTolerance_WithinRange()
    {
        // Stage
        var expected = 100.0;
        var actual = 100.5;
        var tolerance = 1.0;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual, tolerance);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_CustomTolerance_BeyondRange()
    {
        // Stage
        var expected = 100.0;
        var actual = 101.5;
        var tolerance = 1.0;

        // Test - Should throw
        Assert.Throws<AssertFailedException>(() => NumericAsserts.AreSimilar(expected, actual, tolerance));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Int32_CustomTolerance_WithinRange()
    {
        // Stage
        var expected = 100;
        var actual = 105;
        var tolerance = 10;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual, tolerance);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Int32_CustomTolerance_BeyondRange()
    {
        // Stage
        var expected = 100;
        var actual = 115;
        var tolerance = 10;

        // Test - Should throw
        Assert.Throws<AssertFailedException>(() => NumericAsserts.AreSimilar(expected, actual, tolerance));
    }

    #endregion

    #region AreSimilar with Custom Message

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_CustomMessage_Success()
    {
        // Stage
        var expected = 100.0;
        var actual = 100.0;
        var message = "Custom success message";

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual, message);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_CustomMessage_Failure()
    {
        // Stage
        var expected = 100.0;
        var actual = 200.0;
        var customMessage = "Values should be similar";

        // Test
        try
        {
            NumericAsserts.AreSimilar(expected, actual, customMessage);
            Assert.Fail("Should have thrown AssertFailedException");
        }
        catch (AssertFailedException ex)
        {
            // Assert - Verify custom message is in exception
            Assert.Contains(customMessage, ex.Message);
        }
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_ZeroValues()
    {
        // Stage
        var expected = 0.0;
        var actual = 0.0;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_NegativeValues()
    {
        // Stage
        var expected = -123.456789012345;
        var actual = -123.456789012346;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_VerySmallValues()
    {
        // Stage
        var expected = 1.23e-15;
        var actual = 1.23e-15;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_VeryLargeValues()
    {
        // Stage
        var expected = 1.23e15;
        var actual = 1.23e15;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    #endregion
}
