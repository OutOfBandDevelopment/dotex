using OoBDev.ComplexEvents.Common.Schedulers;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Schedulers;
using OoBDev.TestUtilities;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Tests.Schedulers
{
    [TestClass]
    public class ScheduleExecutionTaskBuilderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<ISchedulePersistenceProvider> mockSchedulePersistenceProvider;
        private Mock<IScheduleCalculations> mockScheduleCalculations;
        private Mock<IEventHubSource<ScheduleExecutionProvider>> mockEventHubSource;
        private Mock<IDateTools> mockDateTools;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockSchedulePersistenceProvider = this.mockRepository.Create<ISchedulePersistenceProvider>();
            this.mockScheduleCalculations = this.mockRepository.Create<IScheduleCalculations>();
            this.mockEventHubSource = this.mockRepository.Create<IEventHubSource<ScheduleExecutionProvider>>();
            this.mockDateTools = this.mockRepository.Create<IDateTools>();
        }

        private ScheduleExecutionTaskBuilder CreateScheduleExecutionTaskBuilder(IServiceProvider serviceProvider) =>
            new ScheduleExecutionTaskBuilder(
                serviceProvider,
                this.mockSchedulePersistenceProvider.Object,
                this.mockScheduleCalculations.Object,
                this.mockEventHubSource.Object,
                this.mockDateTools.Object);

        class TestEvent : IEventData
        {
            public DateTimeOffset Request { get; set; }
        }
        [ScheduleAt("test schedule1")]
        [ScheduleAt("test schedule2")]
        [ScheduleAt("test schedule3")]
        class TestSchedule : IComplexEventScheduler
        {
            public Task<IEventData> RequestAsync(DateTimeOffset requestTime) => Task.FromResult<IEventData>(new TestEvent { Request = requestTime });
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task BuildTaskTest()
        {
            // Stage
            var now = DateTime.Now;
            var next = now.AddHours(3);
            var schedules = new[] { "0 */10 * * * MON-FRI" };
            var reference = "test key";
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IComplexEventScheduler, TestSchedule>();

            // Mock
            var mockGetScheduleInstance = mockRepository.Create<IGetScheduleInstance>();

            mockGetScheduleInstance.Setup(s => s.ReferenceKey).Returns(reference);
            mockGetScheduleInstance.Setup(s => s.Scheduler).Returns(typeof(TestSchedule));
            mockGetScheduleInstance.Setup(s => s.Schedules).Returns(schedules);

            mockSchedulePersistenceProvider.Setup(s => s.GetAndLockAsync()).ReturnsAsync(mockGetScheduleInstance.Object);
            mockDateTools.Setup(s => s.Now()).Returns(now);
            mockScheduleCalculations.Setup(s => s.GetNextOccurrence(schedules)).Returns(next);
            mockEventHubSource.Setup(s => s.SendAsync(
                It.IsAny<IEventData>(),
                null,
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()
                )).Returns(Task.FromResult(0));
            //_event
            mockSchedulePersistenceProvider.Setup(s => s.ReleaseAsync(It.Is<IReleaseScheduleInstance>(i => i.ErrorMessage == null))).Returns(Task.FromResult(0));

            // Test
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var scheduleExecutionTaskBuilder = this.CreateScheduleExecutionTaskBuilder(serviceProvider);
            await await scheduleExecutionTaskBuilder.BuildTask();

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task BuildTaskTest_Error()
        {
            // Stage
            var now = DateTime.Now;
            var next = now.AddHours(3);
            var schedules = new[] { "0 */10 * * * MON-FRI" };
            var reference = "test key";
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IComplexEventScheduler, TestSchedule>();

            // Mock
            var mockGetScheduleInstance = mockRepository.Create<IGetScheduleInstance>();

            mockGetScheduleInstance.Setup(s => s.ReferenceKey).Returns(reference);
            mockGetScheduleInstance.Setup(s => s.Scheduler).Returns(typeof(TestSchedule));
            mockGetScheduleInstance.Setup(s => s.Schedules).Returns(schedules);

            mockScheduleCalculations.Setup(s => s.GetNextOccurrence(schedules)).Returns(next);
            mockSchedulePersistenceProvider.Setup(s => s.GetAndLockAsync()).ReturnsAsync(mockGetScheduleInstance.Object);
            mockDateTools.Setup(s => s.Now()).Throws(new Exception());
            mockSchedulePersistenceProvider.Setup(s => s.ReleaseAsync(It.Is<IReleaseScheduleInstance>(i => i.ErrorMessage != null))).Returns(Task.FromResult(0));

            // Test
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var scheduleExecutionTaskBuilder = this.CreateScheduleExecutionTaskBuilder(serviceProvider);
            await await scheduleExecutionTaskBuilder.BuildTask();

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }

        /*
  using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var scheduledItem = await _persistence.GetAndLockAsync().ConfigureAwait(false);

            try
            {
                var scheduler = ActivatorUtilities.CreateInstance<IComplexEventScheduler>(_services, scheduledItem.Scheduler);
                var eventMessage = await scheduler.RequestAsync(_date.Now()).ConfigureAwait(false);
                await _event.SendAsync(eventMessage).ConfigureAwait(false);

                var next = _calculator.GetNextOccurrence(scheduledItem.Schedules);
                await _persistence.ReleaseAsync(new UpdateScheduleInstance
                {
                    Scheduler = scheduledItem.Scheduler,
                    NextStart = next,
                    ErrorMessage = null
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var next = _calculator.GetNextOccurrence(scheduledItem.Schedules);
                await _persistence.ReleaseAsync(new UpdateScheduleInstance
                {
                    Scheduler = scheduledItem.Scheduler,
                    NextStart = next,
                    ErrorMessage = ex.Message,
                }).ConfigureAwait(false);
            }

            transaction.Complete();
        */
    }
}
