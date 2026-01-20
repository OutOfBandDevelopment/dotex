using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Abstractions.Schedulers.Engine
{
    public interface IScheduleExecutionTaskBuilder
    {
        Task<Task> BuildTask();
    }
}
