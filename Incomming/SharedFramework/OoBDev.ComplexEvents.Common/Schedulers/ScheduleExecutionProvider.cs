using OoBDev.ComplexEvents.Common.Schedulers.Models;
using OoBDev.ComplexEvents.Contracts.Schedulers;
using OoBDev.ComplexEvents.Contracts.Schedulers.Engine;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Schedulers
{
    public class ScheduleExecutionProvider : IScheduleExecutionProvider
    {
        private readonly IServiceProvider _services;
        private readonly ISchedulePersistenceProvider _persistence;
        private readonly IScheduleCalculations _calculator;
        private readonly IScheduleExecutionTaskBuilder _builder;

        public ScheduleExecutionProvider(
            IServiceProvider services,
            ISchedulePersistenceProvider persistence,
            IScheduleCalculations calculator,
            IScheduleExecutionTaskBuilder builder
            )
        {
            _services = services;
            _persistence = persistence;
            _calculator = calculator;
            _builder = builder;
        }

        public async Task RegisterAllAsync()
        {
            var schedulers = _services.GetServices<IComplexEventScheduler>();
            var registered = from scheduler in schedulers
                             let schedulerType = scheduler.GetType()
                             let scheduleAttributes = schedulerType.GetCustomAttributes<ScheduleAtAttribute>()
                             let schedules = scheduleAttributes?.Select(s => s.DefaultSchedule).ToArray() ?? Array.Empty<string>()
                             select new
                             {
                                 schedulerType,
                                 schedules,
                             };

            var registeredWithNext = from r in registered
                                     let next = _calculator.GetNextOccurrence(r.schedules)
                                     select new RegisterScheduleInstance
                                     {
                                         Scheduler = r.schedulerType,
                                         Schedules = r.schedules,
                                         NextStart = next,
                                     };

            await _persistence.RegisterIfNotExistAsync(registeredWithNext).ConfigureAwait(false);
        }

        public async Task ExecuteAsync()
        {
            var tasks = new List<Task>();

            while (await _persistence.GetPendingCountAsync() > 0)
            {
                var subTask = await _builder.BuildTask();
                if (!subTask.IsCompleted)
                {
                    tasks.Add(subTask);
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}

