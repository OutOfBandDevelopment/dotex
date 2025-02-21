using OoBDev.Extensions.Reflection;
using OoBDev.System.ResponseModel;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace OoBDev.System.Tests.Reflection;

[TestClass]
public class ReflectionExtensionsTests
{
    public required TestContext TestContext { get; set; }

    public static IEnumerable<object?[]> MakeSafeTestData() => new[]
    {
        new object?[]{typeof(string), "Hello World", "Hello World" },
        [typeof(string), 1, "1"],
        [typeof(byte[]), Convert.ToBase64String("ABC"u8.ToArray()), "ABC"u8.ToArray()],

        [typeof(Guid), "18AACB9C-2989-4322-A490-C7167BEA0DB4", new Guid("18AACB9C-2989-4322-A490-C7167BEA0DB4")],
        [typeof(Guid?), "18AACB9C-2989-4322-A490-C7167BEA0DB4", new Guid("18AACB9C-2989-4322-A490-C7167BEA0DB4")],
        [typeof(Guid), "nope", null],
        [typeof(Guid?), "nope", null],

        [typeof(int), "1", 1],
        [typeof(int?), "1", 1],
        [typeof(int), "nope", null],
        [typeof(int?), "nope", null],
        [typeof(int), 1.1d, 1],

        [typeof(double), "1", 1d],
        [typeof(double?), "1", 1d],
        [typeof(double), "nope", null],
        [typeof(double?), "nope", null],

        [typeof(decimal), "1", 1m],
        [typeof(decimal?), "1", 1m],
        [typeof(decimal), "nope", null],
        [typeof(decimal?), "nope", null],
        [typeof(decimal), 1.0d, 1m],
        [typeof(decimal), 1L, 1m],
        [typeof(decimal), (byte)1, 1m],
        [typeof(decimal), (short)1, 1m],

        [typeof(DateTime), new DateTime(2022,3,16).ToString(), new DateTime(2022,3,16)],
        [typeof(DateTimeOffset), new DateTimeOffset(new DateTime(2022, 3, 16), new TimeSpan(5,0,0)).ToString(), new DateTimeOffset(new DateTime(2022, 3, 16), new TimeSpan(5,0,0))],
        [typeof(DateTime), "3/16/2022", new DateTime(2022,3,16)],
        [typeof(TimeSpan), "16:00", new TimeSpan(16,0,0)],

        [typeof(AttributeTargets), AttributeTargets.Enum, AttributeTargets.Enum],
        [typeof(AttributeTargets), AttributeTargets.Enum.ToString(), AttributeTargets.Enum],
        [typeof(AttributeTargets), (int)AttributeTargets.Enum, AttributeTargets.Enum],
        [typeof(AttributeTargets?), AttributeTargets.Enum, AttributeTargets.Enum],
        [typeof(AttributeTargets), "nope", null],
        [typeof(AttributeTargets?), "nope", null],
    };

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DynamicData(nameof(MakeSafeTestData), DynamicDataSourceType.Method)]
    public void MakeSafeTest(Type? type, object? input, object? expected)
    {
        var capture = new CaptureResultMessage();

        var result = ReflectionExtensions.MakeSafe(type, input, capture);

        if (expected is not string && expected is Array expectedCollection && result is Array resultCollection)
        {
            CollectionAssert.AreEquivalent(expectedCollection, resultCollection);
        }
        else
        {
            Assert.AreEqual(expected, result);
        }

        foreach (var item in capture.Capture())
            TestContext.WriteLine(item.ToString());
    }

    public static IEnumerable<object?[]> MakeSafeArrayTestData() => new[]
    {
        new object[]{typeof(string), new object[] { "Hello World", 1 },new[] { "Hello World", "1" } },
        [typeof(string), Array.Empty<object>(), Array.Empty<string>()],
        [typeof(decimal), new object[] { 1,2.3,2.4m, "1.2" }, new decimal[] {1m, 2.3m,2.4m,1.2m, }],
    };

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DynamicData(nameof(MakeSafeArrayTestData), DynamicDataSourceType.Method)]
    public void MakeSafeArrayTest(Type? type, Array? input, Array? expected)
    {
        var capture = new CaptureResultMessage();

        var result = (Array?)ReflectionExtensions.MakeSafeArray(type, input, capture);
        CollectionAssert.AreEquivalent(expected, result);

        foreach (var item in capture.Capture())
            TestContext.WriteLine(item.ToString());
    }

    public static IEnumerable<object?[]> TryParseTestData() => new[]
    {
        new object?[]{typeof(Guid), "18AACB9C-2989-4322-A490-C7167BEA0DB4", true, new Guid("18AACB9C-2989-4322-A490-C7167BEA0DB4") },
        [typeof(Guid?), "18AACB9C-2989-4322-A490-C7167BEA0DB4", true, new Guid("18AACB9C-2989-4322-A490-C7167BEA0DB4")],
        [typeof(Guid), "nope", false, null],
        [typeof(Guid?), "nope", false, null],

        [typeof(int), "1", true, 1],
        [typeof(int?), "1", true, 1],
        [typeof(int), "nope", false, null],
        [typeof(int?), "nope", false, null],

        [typeof(double), "1", true, 1d],
        [typeof(double?), "1", true, 1d],
        [typeof(double), "nope", false, null],
        [typeof(double?), "nope", false, null],

        [typeof(decimal), "1", true, 1m],
        [typeof(decimal?), "1", true, 1m],
        [typeof(decimal), "nope", false, null],
        [typeof(decimal?), "nope", false, null],

        [typeof(AttributeTargets), AttributeTargets.Enum.ToString(), true, AttributeTargets.Enum],
    };

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DynamicData(nameof(TryParseTestData), DynamicDataSourceType.Method)]
    public void TryParseTest(Type? type, string? input, bool passed, object? expected)
    {
        var capture = new CaptureResultMessage();

        var output = ReflectionExtensions.TryParse(type, input, out var result, capture);
        Assert.AreEqual(passed, output);
        Assert.AreEqual(expected, result);

        foreach (var item in capture.Capture())
            TestContext.WriteLine(item.ToString());
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow(typeof(string), "System.String, System.Private.CoreLib")]
    [DataRow(typeof(ReflectionExtensionsTests), "OoBDev.System.Tests.Reflection.ReflectionExtensionsTests, OoBDev.System.Tests")]
    public void GetShortTypeNameTests(Type type, string expected) => Assert.AreEqual(type.GetShortTypeName(), expected);
}
