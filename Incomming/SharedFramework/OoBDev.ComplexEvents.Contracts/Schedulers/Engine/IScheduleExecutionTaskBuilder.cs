using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Contracts.Schedulers.Engine
{
    public interface IScheduleExecutionTaskBuilder
    {
        Task<Task> BuildTask();
    }
}
