# SourceApp Complex Event Framework

## Summary

The intent of this framework is to reduce implementation and configuration required for handing 
business events/complex events.

## Complex Event Handlers

IComplexEventHandler is an interface for receiving complex event messages.  Your object must be 
registered in the IOC for this interface

### Handler Registration Example

```csharp
    public IServiceCollection AddServices(IServiceCollection services) => services
        .AddTransient<IComplexEventHandler, YourEventHandler>()
        ;
```

### Handler Implmentation Example

```csharp

    [ComplexEventHandler(TargetType = typeof(YourEvent))]
    public class YourEventHandler : IComplexEventHandler<YourEvent>
    {
        public Task HandleEvent(YourEvent message)
        {
            //TODO: put your for your business event here
            return Task.FromResult(0);
        }
    }
```

## Manual Events 

To explicitly trigger a complex event call the [IEventHubSource<>::SendAsync()] function.

```csharp
    public class YourManager : IYourManager
    {
        private readonly IEventHubSource<YourManager> _eventHub; //Note: the generic type parameter should match this class

        public YourManager(
            IEventHubSource<YourManager> eventHub)
        {
            _eventHub = eventHub;
        }

        public async Task DoWork(...)
        {
            await _eventHub.SendAsync(new YourEvent{...}).ConfigureAwait(false);
        }
    }
```

## Scheduled Events

If you need your event called on a scheduled basis you can use the IComplexEventScheduler with the ScheduledAtAttribute interface.

The ScheduleAt string input value is the cron format WithSeconds parsed by [NCrontab.Advanced](https://github.com/jcoutch/NCrontab-Advanced)

```CronStringFormat.WithSeconds
CronStringFormat.WithSeconds: SECONDS MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK
```

This has an optional extension for timezones as follows.

```CronStringFormat.WithSeconds+TimezoneId
CronStringFormat.WithSeconds: SECONDS MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK {SYSTEM-TIMEZONEINFO-ID}
```

The `{}` characters are required and the string to be substituted should match a value from [TimeZoneInfo.Id](https://docs.microsoft.com/en-us/dotnet/api/system.timezoneinfo.id)

### Notes

* You may return null to skip this execution. 
* The object returned will be published on the IEventHubSource<> allowing any registered handler for that type to process event
* The default timezone used for the schedules is UTC (Coordinated Universal Time)


### Override timezone offset

By default the provided schedules are in UTC (Coordinated Universal Time).  If you wish to scheduled the events for a different timezone the 
schedule string format has been updated to include a timezone id as defined in the system timezone table. This is using the .Net 
[TimeZoneInfo.ConvertTimeBySystemTimeZoneId](https://docs.microsoft.com/en-us/dotnet/api/system.timezoneinfo.converttimebysystemtimezoneid)
function to resolve the offset.  

An example schedule string with a timezone would be declared as `0 0 12 * * * {Eastern Standard Time}` would execute as 12:00pm EST.    

### Scheduler Registration Example

```csharp
    public IServiceCollection AddServices(IServiceCollection services) => services
        .AddTransient<IComplexEventScheduler, YourEventHandler>()
        ;
```

### Scheduler Implmentation Example

```csharp
    [ScheduleAt("0 0 12 * * Tue")] //Note: the string in the NCronTab.Advanced format WithSeconds
    [ScheduleAt("0 0 10 * * Mon,Fri")]
    public class YourEventScheduler : IComplexEventScheduler
    {
        public Task<object> GenerateAsync(DateTimeOffset requestTime) => new YourEvent();
    }
```

## Example Event

```csharp
    public class YourEvent : IEventData
    {
        //TODO: define your event as required.. should be as small as possible
    }
```
