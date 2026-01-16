using OoBDev.Generations.Rules;

namespace OoBDev.Generations.Tests.TestTargets
{
    public class FakePersonModel
    {
#pragma warning disable CS8618

        [FirstSpaceLastName]
        public string FirstLast { get; set; }
        [LastCommaFirstName]
        public string LastFirst { get; set; }
        [Words(ParagraphCount = 1)]
        public string Comment { get; set; }
#pragma warning restore CS8618
    }
}
