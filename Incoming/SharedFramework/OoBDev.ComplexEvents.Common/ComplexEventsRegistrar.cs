using OoBDev.ComplexEvents.Common.Resolvers;
using OoBDev.ComplexEvents.Common.Schedulers;
using OoBDev.ComplexEvents.Common.Services;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Schedulers;
using OoBDev.ComplexEvents.Contracts.Schedulers.Engine;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.ComplexEvents.Common
{
    public class ComplexEventsRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<IComplexEventHandlerResolver, ComplexEventHandlerResolver>();
            services.TryAddTransient<IComplexEventHandlerFactory, ComplexEventHandlerFactory>();
            services.TryAddTransient<IEventResolver, EventHubResolver>();
            
            services.TryAddTransient<IScheduleExecutionProvider, ScheduleExecutionProvider>();
            services.TryAddTransient<IScheduleCalculations, ScheduleCalculations>();
            services.TryAddTransient<IScheduleExecutionTaskBuilder, ScheduleExecutionTaskBuilder>();
            
            services.TryAddTransient(typeof(IEventHubSource<>), typeof(EventHubSource<>));
            services.AddTransient(typeof(IEventHubProvider<>), typeof(EnqueueEventHubProvider<>));

            return services;
        }
    }
}