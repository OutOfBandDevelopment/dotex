namespace OoBDev.Generations.Tests.TestTargets
{
    public abstract class IncompleteClassBase
    {
        public int Local { get; set; } = 1;
        public virtual int Virtual { get; set; } = 2;
        public abstract int Abstract { get; set; }

        public string LocalFunction() => "Local Function";
        public virtual string VirtualFunction() => "Virtual Function";
        public abstract string AbstractFunction();
        public abstract string AbstractFunction2(string input1);
        public abstract void AbstractFunction3(string input1, string input2);
    }
}
