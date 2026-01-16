using OoBDev.Generations.Rules;

namespace OoBDev.Generations.Tests.TestTargets
{
    public class FakeEntity2
    {
#pragma warning disable CS8618

        [SpecialGeneration]
        public string Prop1 { get; set; }
        [FirstSpaceLastName]
        public string Prop2 { get; set; }
#pragma warning restore CS8618
    }
}
