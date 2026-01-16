namespace OoBDev.Generations.Tests.TestTargets
{
    public interface ITargetInterface
    {
        string GetterProperty { get; }
        string SetterProperty { set; }
        string GetterSetterProperty { get; set; }

        ModelWithProperties2 DoWork(string a, int b);
    }
}
