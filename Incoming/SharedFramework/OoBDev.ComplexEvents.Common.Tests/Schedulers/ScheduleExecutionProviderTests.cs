using OoBDev.ComplexEvents.Common.Schedulers;
using OoBDev.ComplexEvents.Common.Tests.Entities;
using OoBDev.ComplexEvents.Common.Tests.Examples;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Schedulers;
using OoBDev.ComplexEvents.Contracts.Schedulers.Engine;
using OoBDev.ComplexEvents.EntityFrameworkCore;
using OoBDev.EntityFrameworkCore.EmbeddedResources;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Assertions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Tests.Schedulers
{
    [TestClass]
    public class ScheduleExecutionProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<ISchedulePersistenceProvider> mockSchedulePersistenceProvider;
        private Mock<IScheduleCalculations> mockScheduleCalculations;
        private Mock<IScheduleExecutionTaskBuilder> mockScheduleExecutionTaskBuilder;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockSchedulePersistenceProvider = this.mockRepository.Create<ISchedulePersistenceProvider>();
            this.mockScheduleCalculations = this.mockRepository.Create<IScheduleCalculations>();
            this.mockScheduleExecutionTaskBuilder = this.mockRepository.Create<IScheduleExecutionTaskBuilder>();
        }

        private ScheduleExecutionProvider CreateProvider(IServiceProvider serviceProvider) =>
            new ScheduleExecutionProvider(
                serviceProvider,
                this.mockSchedulePersistenceProvider.Object,
                this.mockScheduleCalculations.Object,
                this.mockScheduleExecutionTaskBuilder.Object);

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
        public async Task RegisterAllAsyncTest()
        {
            // Stage
            var now = DateTimeOffset.Now;
            var serviceProvider = new ServiceCollection()
                .AddTransient<IComplexEventScheduler, TestSchedule>()
                .BuildServiceProvider()
                ;

            // Mock
            mockScheduleCalculations.Setup(s => s.GetNextOccurrence(It.IsAny<string[]>())).Returns(now);
            mockSchedulePersistenceProvider.Setup(s => s.RegisterIfNotExistAsync(It.IsAny<IEnumerable<IRegisterScheduleInstance>>()))
                                           .Callback<IEnumerable<IRegisterScheduleInstance>>(i => i.ToArray())
                                           .Returns(Task.FromResult(0));

            // Test
            var provider = this.CreateProvider(serviceProvider);
            await provider.RegisterAllAsync();

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ExecuteAsyncTest()
        {
            // Stage
            var now = DateTimeOffset.Now;
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider()
                ;

            // Mock
            mockSchedulePersistenceProvider.SetupSequence(s => s.GetPendingCountAsync())
                .Returns(Task.FromResult(1))
                .Returns(Task.FromResult(0))
                ;
            mockScheduleExecutionTaskBuilder.Setup(s => s.BuildTask()).Returns(Task.FromResult<Task>(Task.FromResult(0)));

            // Test
            var provider = this.CreateProvider(serviceProvider);

            await provider.ExecuteAsync();

            // Assert
            //Note: there is no result

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        public async Task ExecuteAsyncTest_Simulation()
        {
            // Stage
            var serviceProvider = new ServiceCollection()
                .AddDebugTestConfigurations()
                .AddDebugTestServices(this.TestContext, Microsoft.Extensions.Logging.LogLevel.Warning)
                .AddSharedFrameworkServices()
                .AddSingleton<ISchedulePersistenceProvider, SchedulePersistenceProvider<TestDbContext>>()

                .AddDbContext<TestDbContext>((sp, opt) => opt
                    .UseEmbeddedResourceDatabase(serviceProvider: sp, scope: this)
                    .ConfigureWarnings( warn=>warn
                        .Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)
                    )
                )
                .BuildServiceProvider()
                ;

            var seeded = serviceProvider.GetRequiredService<IEmbeddedResourceSeedResults>();
            seeded.OnExecuteDbDataReader = c =>
            {
                this.TestContext?.WriteLine($"OnExecuteDbDataReader: {c.CommandText}");


                if (c.CommandText == "[_Scheduler].[GetRecord]")
                    return new object[][]{
                        new object[]
                        {
                            new 
                            {
                                EventGeneratorId = 1,
                                AssemblyQualifiedName = "OoBDev.Core.Business.ComplexEvents.Scheduler.StudentWithdrawnScheduler, OoBDev.Core.Business",
                                OriginalSchedule = "0 0 6 * * * {Eastern Standard Time}",
                                SchedulesXml = "<X><S>0 0 6 * * * {Eastern Standard Time}</S></X>",
                            }
                        }
                    };


                return new object[][]
                {
                    new object[] {
                        new {HelloWorld = "Hi"},
                    }
                };
            };
            seeded.OnExecuteNonQuery = c =>
            {
                this.TestContext?.WriteLine($"OnExecuteNonQuery: {c.CommandText}");
                return -999;
            };
            var cnt = 1;
            seeded.OnExecuteScalar = c =>
            {
                this.TestContext?.WriteLine($"OnExecuteScalar: {c.CommandText}");

                if (c.CommandText == "[_Scheduler].[PendingCount]") 
                    return cnt--;

                return new { Hello = "World" };
            };

            var collector = serviceProvider.GetRequiredService<ITestResultsCollector>();

            // Test
            var provider = ActivatorUtilities.CreateInstance<ScheduleExecutionProvider>(serviceProvider);
            await provider.ExecuteAsync();

            // Assert
            JsonComparer.AssertSummaryDifferences(TestContext, collector);
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        public async Task RegisterAllAsyncTest_Simulation()
        {
            // Stage
            var serviceProvider = new ServiceCollection()
                .AddDebugTestConfigurations()
                .AddDebugTestServices(this.TestContext)
                .AddSharedFrameworkServices()
                .AddSingleton<ISchedulePersistenceProvider, SchedulePersistenceProvider<TestDbContext>>()
                .AddDbContext<TestDbContext>((sp, opt) => opt
                    .UseEmbeddedResourceDatabase(serviceProvider: sp, scope: this)
                    .ConfigureWarnings(warn => warn
                       .Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)
                    )
                )
                .BuildServiceProvider()
                ;

            var seeded = serviceProvider.GetRequiredService<IEmbeddedResourceSeedResults>();
            var collector = serviceProvider.GetRequiredService<ITestResultsCollector>();

            // Test
            var provider = ActivatorUtilities.CreateInstance<ScheduleExecutionProvider>(serviceProvider);
            await provider.RegisterAllAsync();

            // Assert
            JsonComparer.AssertSummaryDifferences(TestContext, collector);
        }
    }
}
