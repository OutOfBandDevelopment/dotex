using System;

namespace OoBDev.DataLoader.PipeLine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataPipelinePriorityAttribute : Attribute
    {
        public DataPipelinePriorityAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; }
    }
}
