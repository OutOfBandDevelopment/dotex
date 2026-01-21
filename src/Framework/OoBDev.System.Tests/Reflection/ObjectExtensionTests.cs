using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Reflection;
using OoBDev.TestUtilities;
using System;
using System.Collections;

namespace OoBDev.System.Tests.Reflection;

[TestClass]
public class ObjectExtensionTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("1", typeof(int), 1)]
    [DataRow("1.0", typeof(int), 1)]
    [DataRow("1.0", typeof(int?), 1)]
    [DataRow("1.5", typeof(int), 2)]
    [DataRow("1", typeof(uint), 1u)]
    [DataRow("1.0", typeof(uint), 1u)]
    [DataRow("1.5", typeof(uint), 2u)]
    [DataRow("1", typeof(long), 1L)]
    [DataRow("1.0", typeof(long), 1L)]
    [DataRow("1.5", typeof(long), 2L)]
    [DataRow("1", typeof(ulong), 1ul)]
    [DataRow("1.0", typeof(ulong), 1ul)]
    [DataRow("1.5", typeof(ulong), 2ul)]
    [DataRow("1", typeof(string), "1")]
    [DataRow("1.0", typeof(string), "1.0")]
    [DataRow(1, typeof(string), "1")]
    [DataRow(1.0, typeof(string), "1")]
    [DataRow(1.5, typeof(string), "1.5")]
    [DataRow("1", typeof(double), 1d)]
    [DataRow("1.0", typeof(double), 1.0d)]
    [DataRow("1.5", typeof(double), 1.5d)]
    [DataRow(new byte[] { 1, 2, 3, 4, 5 }, typeof(string), "AQIDBAU=")]
    [DataRow("AQIDBAU=", typeof(byte[]), new byte[] { 1, 2, 3, 4, 5 })]
    public void Test(object input, Type type, object expected)
    {
        var method = typeof(ObjectExtensions).GetMethod(nameof(ObjectExtensions.As), [typeof(object)]).MakeGenericMethod(type);
        var result = method.Invoke(null, [input]);

        if (expected is IEnumerable and not string)
        {
            CollectionAssert.AreEqual((ICollection)expected, (ICollection)result);
        }
        else
        {
            Assert.AreEqual(expected, result);
        }
    }

}
