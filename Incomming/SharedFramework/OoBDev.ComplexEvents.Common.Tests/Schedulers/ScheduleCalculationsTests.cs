#nullable enable

using OoBDev.TestUtilities;
using OoBDev.ComplexEvents.Common.Schedulers;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using OoBDev.Toolkit.Common;

namespace OoBDev.ComplexEvents.Common.Tests.Schedulers
{
    [TestClass]
    public class ScheduleCalculationsTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TestContext? TestContext { get; set; }
        private MockRepository mockRepository;
        private Mock<IDateTools> mockDateTools;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDateTools = this.mockRepository.Create<IDateTools>();
        }

        private ScheduleCalculations CreateScheduleCalculations() => new ScheduleCalculations(this.mockDateTools.Object);

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetNextOccurrenceTest()
        {
            // Stage
            var time = DateTimeOffset.UtcNow;
            var schedule = "0 0 17 * * *";
            var later = time.AddHours(2);
            DateTime realresult = DateTime.MinValue;

            // Mock
            mockDateTools.Setup(s => s.UtcNow()).Returns(time);
            mockDateTools.Setup(s => s.ConvertToTimeZoneId(It.IsAny<DateTimeOffset>(), It.IsAny<string>())).Returns(later);
            mockDateTools.Setup(s => s.ConvertToTimeZoneId(It.IsAny<DateTime>(), It.IsAny<string>()))
                         .Callback<DateTime, string>((d, s) => realresult = d)
                         .Returns(later);

            // Test
            var scheduleCalculations = this.CreateScheduleCalculations();

            var result = scheduleCalculations.GetNextOccurrence(schedule);

            this.TestContext?.WriteLine($"current:  {time}");
            this.TestContext?.WriteLine($"schedule: {schedule}");
            this.TestContext?.WriteLine($"result:   {result}");
            this.TestContext?.WriteLine($"realresult:   {realresult}");

            // Assert
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(0, realresult.Second);
            Assert.AreEqual(0, realresult.Minute);
            Assert.AreEqual(17, realresult.Hour);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [DataTestMethod, TestCategory(TestCategories.Simulation)]
        [DataRow("Turks And Caicos Standard Time")]
        [DataRow("Paraguay Standard Time")]
        [DataRow("Hawaiian Standard Time")]
        [DataRow("Morocco Standard Time")]
        [DataRow("Pacific Standard Time")]
        [DataRow("Eastern Standard Time")]
        [DataRow((string?)null)]
        public void GetNextOccurrenceTest_Sim(string? timezone)
        {
            // Stage
            var time = DateTimeOffset.Now;
            var schedule = $"0 0 17 * * * {{{timezone}}}";

            // Mock

            // Test
            var scheduleCalculations = new ScheduleCalculations(
                new DateTools()
                );

            var result = scheduleCalculations.GetNextOccurrence(schedule);

            this.TestContext?.WriteLine($"current:  {time}");
            this.TestContext?.WriteLine($"schedule: {schedule}");
            this.TestContext?.WriteLine($"result:   {result}");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(0, result.Value.Second);
            Assert.AreEqual(0, result.Value.Minute);
            Assert.AreEqual(17, result.Value.Hour);

            // Verify
        }
    }
}
