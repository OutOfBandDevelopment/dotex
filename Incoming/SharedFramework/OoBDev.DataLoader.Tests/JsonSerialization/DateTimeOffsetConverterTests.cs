using OoBDev.DataLoader.JsonSerialization;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace OoBDev.DataLoader.Tests.JsonSerialization
{
    [TestClass]
    public class DateTimeOffsetConverterTests
    {
        public TestContext? TestContext { get; set; }

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow(typeof(DateTimeOffset), true)]
        [DataRow(typeof(DateTimeOffset?), true)]
        [DataRow(typeof(DateTime), true)]
        [DataRow(typeof(DateTime?), true)]
        [DataRow(typeof(string), false)]
        [DataRow(typeof(TimeSpan), false)]
        [DataRow(typeof(TimeSpan?), false)]
        [DataRow(typeof(object), false)]
        public void CanConvertTest(Type objectType, bool expected)
        {
            //Test
            var dateTimeOffsetConverter = new DateTimeOffsetConverter();
            var result = dateTimeOffsetConverter.CanConvert(objectType);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [ExpectedException(typeof(NotSupportedException))]
        public void WriteJsonTest()
        {
            //Test
            var dateTimeOffsetConverter = new DateTimeOffsetConverter();
            dateTimeOffsetConverter.WriteJson(
                new JsonTextWriter(new StreamWriter(new MemoryStream())), 
                new { }, 
                new JsonSerializer());

            // Assert
            Assert.Fail("You shouldn't get here");
        }

        public class TargetObject
        {
            public DateTimeOffset Time { get; set; }
        }

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow("{time:'12/1/2022 1:23:45'}", null, "12/1/2022 1:23:45")]

        [DataRow("{time:'TODAY+0@1:1'}", null, null)]
        [DataRow("{time:'TODAY-0@1:1'}", null, null)]
        [DataRow("{time:'TODAY+1@1:1'}", null, null)]
        [DataRow("{time:'TODAY-1@1:1'}", null, null)]

        [DataRow("{time:'WEEKSTART+0@1:1'}", null, null)]
        [DataRow("{time:'WEEKSTART-0@1:1'}", null, null)]
        [DataRow("{time:'WEEKSTART+1@1:1'}", null, null)]
        [DataRow("{time:'WEEKSTART-1@1:1'}", null, null)]
        [DataRow("{time:'WEEKSTART+7@1:1'}", null, null)]
        [DataRow("{time:'WEEKSTART-7@1:1'}", null, null)]

        public void ReadJsonTest(string input, string? timeOffsetString, string? expectedString)
        {
            var expected = DateTimeOffset.TryParse(expectedString, out var dto) ? (DateTimeOffset?)dto : null;
            var timeOffset = TimeSpan.TryParse(timeOffsetString, out var ts) ? (TimeSpan?)ts : null;

            var jobj = JObject.Parse(input);
            var reader = jobj.CreateReader();

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new DateTimeOffsetConverter());
            var result = serializer.Deserialize<TargetObject>(reader)?? throw new NotSupportedException();

            this.TestContext?.WriteLine($"{result.Time}");

            var now = DateTimeOffset.Now;
            var today = now - now.TimeOfDay;
            var checkOffset = result.Time - today;

            this.TestContext?.WriteLine($"Now: {now}");
            this.TestContext?.WriteLine($"Result: {result.Time}");
            this.TestContext?.WriteLine($"Today: {today}");
            this.TestContext?.WriteLine($"CheckOffset: {checkOffset}");

            // Assert
            if (expected.HasValue)
            {
                Assert.AreEqual(result.Time, expected.Value);
            }
            if (timeOffset.HasValue)
            {
                Assert.AreEqual(timeOffset.Value.Days, checkOffset.Days);
                Assert.AreEqual(timeOffset.Value.Hours, checkOffset.Hours);
                Assert.AreEqual(timeOffset.Value.Minutes, checkOffset.Minutes);
                Assert.AreEqual(timeOffset.Value.Seconds, checkOffset.Seconds);
            }

            if (!expected.HasValue && !timeOffset.HasValue)
            {
                Assert.Inconclusive("This has not check the results");
            }
        }
    }
}
