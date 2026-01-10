using OoBDev.ComplexEvents.Common.Schedulers.Models;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Schedulers;
using OoBDev.ComplexEvents.Contracts.Schedulers.Engine;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace OoBDev.ComplexEvents.Common.Schedulers
{
    public class ScheduleExecutionTaskBuilder : IScheduleExecutionTaskBuilder
    {
        private readonly IServiceProvider _services;
        private readonly ISchedulePersistenceProvider _persistence;
        private readonly IScheduleCalculations _calculator;
        private readonly IEventHubSource<ScheduleExecutionProvider> _event;
        private readonly IDateTools _date;

        public ScheduleExecutionTaskBuilder(
            IServiceProvider services,
            ISchedulePersistenceProvider persistence,
            IScheduleCalculations calculator,
            IEventHubSource<ScheduleExecutionProvider> @event,
            IDateTools date
            )
        {
            _services = services;
            _persistence = persistence;
            _calculator = calculator;
            _event = @event;
            _date = date;
        }

        public async Task<Task> BuildTask()
        {
            var transaction = new TransactionScope(
               TransactionScopeOption.RequiresNew,
               new TransactionOptions
               {
                   IsolationLevel = IsolationLevel.ReadCommitted,
               },
               TransactionScopeAsyncFlowOption.Enabled);

            var scheduledItem = await _persistence.GetAndLockAsync().ConfigureAwait(false);
            if (scheduledItem == null)
            {
                //nothing to do
                return Task.FromResult(0);
            }

            return Task.Run(async () =>
            {
                try
                {
                    var scheduler = (IComplexEventScheduler)ActivatorUtilities.CreateInstance(_services, scheduledItem.Scheduler);
                    var eventMessage = await scheduler.RequestAsync(_date.Now()).ConfigureAwait(false);
                    await _event.SendAsync(eventMessage).ConfigureAwait(false);

                    var next = _calculator.GetNextOccurrence(scheduledItem.Schedules);
                    await _persistence.ReleaseAsync(new UpdateScheduleInstance(
                        referenceKey: scheduledItem.ReferenceKey,
                        scheduler: scheduledItem.Scheduler,
                        nextStart: next,
                        errorMessage: null
                        )).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var next = _calculator.GetNextOccurrence(scheduledItem.Schedules);
                    await _persistence.ReleaseAsync(new UpdateScheduleInstance(
                        referenceKey: scheduledItem.ReferenceKey,
                        scheduler: scheduledItem.Scheduler,
                        nextStart: next,
                        errorMessage: ex.Message
                        )).ConfigureAwait(false);
                }

                transaction.Complete();
                transaction.Dispose();
            });
        }
    }
}

