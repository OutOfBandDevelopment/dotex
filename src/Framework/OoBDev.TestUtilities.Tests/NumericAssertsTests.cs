using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OoBDev.TestUtilities.Tests;

[TestClass]
public class NumericAssertsTests
{
    public TestContext TestContext { get; set; }

    #region AreSimilar - Double

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_ExactMatch()
    {
        // Stage
        double expected = 123.456;
        double actual = 123.456;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_WithinTolerance()
    {
        // Stage
        double expected = 123.456789012345;
        double actual = 123.456789012346; // Differs in last digit

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_BeyondTolerance()
    {
        // Stage
        double expected = 123.456;
        double actual = 123.457;

        // Test - Should throw
        Assert.ThrowsException<AssertFailedException>(() =>
        {
            NumericAsserts.AreSimilar(expected, actual);
        });
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_LargeValues()
    {
        // Stage
        double expected = -59423282750552.782382789829384;
        double actual = -59423282750552.782382789829386;

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
        float expected = 123.456f;
        float actual = 123.456f;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Float_WithinTolerance()
    {
        // Stage
        float expected = 123.4567f;
        float actual = 123.45671f;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Float_BeyondTolerance()
    {
        // Stage
        float expected = 123.456f;
        float actual = 123.46f;

        // Test - Should throw
        Assert.ThrowsException<AssertFailedException>(() =>
        {
            NumericAsserts.AreSimilar(expected, actual);
        });
    }

    #endregion

    #region AreSimilar - Decimal

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Decimal_ExactMatch()
    {
        // Stage
        decimal expected = 123.456789012345678901234567890m;
        decimal actual = 123.456789012345678901234567890m;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Decimal_WithinTolerance()
    {
        // Stage
        decimal expected = 123.456789012345678901234567890m;
        decimal actual = 123.456789012345678901234567891m;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [ExpectedException(typeof(AssertFailedException))]
    public void AreSimilar_Decimal_BeyondTolerance()
    {
        // Stage
        decimal expected = 123.456m;
        decimal actual = 123.457m;

        // Test - Should throw
        NumericAsserts.AreSimilar(expected, actual);
    }

    #endregion

    #region AreSimilar - Integer Types

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Int32_ExactMatch()
    {
        // Stage
        int expected = 12345;
        int actual = 12345;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [ExpectedException(typeof(AssertFailedException))]
    public void AreSimilar_Int32_NotEqual()
    {
        // Stage
        int expected = 12345;
        int actual = 12346;

        // Test - Should throw
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Int64_ExactMatch()
    {
        // Stage
        long expected = 123456789012345;
        long actual = 123456789012345;

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
        double expected = 100.0;
        double actual = 100.5;
        double tolerance = 1.0;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual, tolerance);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [ExpectedException(typeof(AssertFailedException))]
    public void AreSimilar_Double_CustomTolerance_BeyondRange()
    {
        // Stage
        double expected = 100.0;
        double actual = 101.5;
        double tolerance = 1.0;

        // Test - Should throw
        NumericAsserts.AreSimilar(expected, actual, tolerance);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Int32_CustomTolerance_WithinRange()
    {
        // Stage
        int expected = 100;
        int actual = 105;
        int tolerance = 10;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual, tolerance);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [ExpectedException(typeof(AssertFailedException))]
    public void AreSimilar_Int32_CustomTolerance_BeyondRange()
    {
        // Stage
        int expected = 100;
        int actual = 115;
        int tolerance = 10;

        // Test - Should throw
        NumericAsserts.AreSimilar(expected, actual, tolerance);
    }

    #endregion

    #region AreSimilar with Custom Message

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_CustomMessage_Success()
    {
        // Stage
        double expected = 100.0;
        double actual = 100.0;
        string message = "Custom success message";

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual, message);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_CustomMessage_Failure()
    {
        // Stage
        double expected = 100.0;
        double actual = 200.0;
        string customMessage = "Values should be similar";

        // Test
        try
        {
            NumericAsserts.AreSimilar(expected, actual, customMessage);
            Assert.Fail("Should have thrown AssertFailedException");
        }
        catch (AssertFailedException ex)
        {
            // Assert - Verify custom message is in exception
            Assert.IsTrue(ex.Message.Contains(customMessage));
        }
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_ZeroValues()
    {
        // Stage
        double expected = 0.0;
        double actual = 0.0;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_NegativeValues()
    {
        // Stage
        double expected = -123.456789012345;
        double actual = -123.456789012346;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_VerySmallValues()
    {
        // Stage
        double expected = 1.23e-15;
        double actual = 1.23e-15;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AreSimilar_Double_VeryLargeValues()
    {
        // Stage
        double expected = 1.23e15;
        double actual = 1.23e15;

        // Test & Assert
        NumericAsserts.AreSimilar(expected, actual);
    }

    #endregion
}
