using System;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Contracts.Schedulers
{
    /// <summary>
    /// this interface will create a complex event that will be posted based on the matched ScheduledAt time.
    /// Is this interface is not used then a simple message with {requestTime= DateTimeOffset.Now} will be sent in its place.
    /// </summary>
    /// <example> 
    /// [ScheduleAt("0 */45 6-18 * * MON,WED,FRI")]
    /// [ScheduleAt("0 */30 5-17 * * TUE,THU")]
    /// public class ScheduleEventGenerator : IComplexEventScheduler
    /// {
    ///     public Task&lt;object&gt; GenerateAsync(DateTimeOffset requestTime) => new MySpecialEvent();
    /// }
    /// </example>
    public interface IComplexEventScheduler
    {
        /// <summary>
        /// This may be used to provide the expected complex event request body.
        /// </summary>
        /// <param name="requestTime">DateTimeOffset of the time requested</param>
        /// <returns>The returned object should match the related ComplexEvent type</returns>
        Task<IEventData> RequestAsync(DateTimeOffset requestTime);
    }
}
