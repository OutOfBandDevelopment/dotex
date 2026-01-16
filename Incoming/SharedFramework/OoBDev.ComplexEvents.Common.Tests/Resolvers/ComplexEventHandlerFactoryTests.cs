using OoBDev.ComplexEvents.Common.Resolvers;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.Toolkit.Contracts.Extensions;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OoBDev.Toolkit;

namespace OoBDev.ComplexEvents.Common.Tests.Resolvers
{
    [TestClass]
    public class ComplexEventHandlerFactoryTests
    {
        public TestContext TestContext { get; set; }

        private ComplexEventHandlerFactory CreateFactory(params IComplexEventHandler[] handlers)
        {
            IServiceCollection services = new ServiceCollection()
                .AddDebugTestServices(this.TestContext)
                .AddComplexEventsServices()
                .AddToolkitServices()
                ;

            foreach (var handler in handlers)
                services.AddTransient(typeof(IComplexEventHandler), handler.GetType());

            var serviceProvider = services.BuildServiceProvider();
            var factory = ActivatorUtilities.CreateInstance<ComplexEventHandlerFactory>(serviceProvider);
            return factory;
        }

        public class MatchedEventData : IEventData
        {
        }
        public class UnmatchedEventData : IEventData
        {
        }

        [ComplexEventHandler(TargetType = typeof(MatchedEventData))]
        public class MatchedHandler : IComplexEventHandler
        {
            public Task HandleEvent(object message) => throw new NotImplementedException();
        }
        [ComplexEventHandler(TargetType = typeof(MatchedEventData))]
        [ComplexEventHandler(TargetType = typeof(UnmatchedEventData))]
        public class MatchedAndUnmatchedHandler : IComplexEventHandler
        {
            public Task HandleEvent(object message) => throw new NotImplementedException();
        }
        [ComplexEventHandler(TargetType = typeof(UnmatchedEventData))]
        public class UnmatchHandler : IComplexEventHandler
        {
            public Task HandleEvent(object message) => throw new NotImplementedException();
        }
        [ComplexEventHandler]
        public class MatchAllHandler : IComplexEventHandler
        {
            public Task HandleEvent(object message) => throw new NotImplementedException();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetHandlersTest()
        {
            // Stage
            string target = typeof(MatchedEventData).FullName;

            // Test
            var factory = this.CreateFactory(
                new MatchedHandler(),
                new MatchedAndUnmatchedHandler(),
                new UnmatchHandler(),
                new MatchAllHandler()
                );
            var result = factory.GetHandlers(target).ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
        }
    }
}
